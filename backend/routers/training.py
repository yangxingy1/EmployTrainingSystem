import json
from datetime import datetime

from fastapi import APIRouter, Depends, HTTPException, Query
from sqlalchemy import func
from sqlalchemy.orm import Session
from typing import Optional
from backend.database import get_db
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.models.training_attempt import TrainingAttempt
from backend.models.training_sub_result import TrainingSubResult
from backend.models.user import User
from backend.schemas.training import TrainingCancelRequest, TrainingResultSubmit, TrainingStartRequest
from backend.training_catalog import (
    find_sub_item,
    get_sub_items,
    is_allowed_scene,
    sub_task_display_name,
)

router = APIRouter(prefix="/training", tags=["Training"])


def _json_loads(value, fallback):
    if not value:
        return fallback
    try:
        return json.loads(value)
    except json.JSONDecodeError:
        return fallback


def _error_summary(errors: list[dict]) -> str:
    if not errors:
        return "无关键错误"
    parts = []
    for error in errors[:3]:
        reason = error.get("reason") or "未知错误"
        consequence = error.get("consequence") or "暂无后果说明"
        step_name = error.get("stepName") or error.get("step_name") or "未标记步骤"
        parts.append(f"{step_name}：{reason}；后果：{consequence}")
    if len(errors) > 3:
        parts.append(f"另有 {len(errors) - 3} 条错误，展开子项目查看详情。")
    return "\n".join(parts)


def _sub_result_payload(result: TrainingSubResult):
    return {
        "id": result.id,
        "attempt_id": result.attempt_id,
        "sub_task_id": result.sub_task_id,
        "sub_task_name": result.sub_task_name,
        "status": result.status,
        "score": result.score,
        "train_time": result.train_time,
        "started_at": result.started_at,
        "finished_at": result.finished_at,
        "steps": _json_loads(result.steps_json, []),
        "errors": _json_loads(result.errors_json, []),
        "details": _json_loads(result.details_json, None),
        "error_count": result.error_count or 0,
        "safety_error_count": result.safety_error_count or 0,
        "summary": result.summary or "",
        "ai_ready": False,
    }


def _legacy_sub_result(attempt: TrainingAttempt):
    if not attempt.sub_task_id and not attempt.details_json:
        return None
    details = _json_loads(attempt.details_json, {})
    steps = details.get("steps") or []
    errors = details.get("errors") or []
    return {
        "id": None,
        "attempt_id": attempt.id,
        "sub_task_id": attempt.sub_task_id or details.get("sub_task_id") or "legacy_result",
        "sub_task_name": sub_task_display_name(attempt.scene_name, attempt.sub_task_id),
        "status": "done",
        "score": attempt.score,
        "train_time": attempt.train_time,
        "started_at": details.get("started_at") or str(attempt.started_at or ""),
        "finished_at": details.get("finished_at") or str(attempt.finished_at or ""),
        "steps": steps,
        "errors": errors,
        "details": details,
        "error_count": attempt.error_count or len(errors),
        "safety_error_count": attempt.safety_error_count or sum(1 for e in errors if e.get("severity") == "safety"),
        "summary": attempt.summary or _error_summary(errors),
        "ai_ready": False,
    }


def _attempt_payload(
    db: Session,
    attempt: TrainingAttempt,
    task: Optional[Task]  = None,
    user: Optional[User] = None,
):
    catalog = get_sub_items(attempt.scene_name)
    rows = db.query(TrainingSubResult).filter(
        TrainingSubResult.attempt_id == attempt.id
    ).order_by(TrainingSubResult.id.asc()).all()
    completed_by_id = {row.sub_task_id: _sub_result_payload(row) for row in rows}

    sub_results = []
    if catalog:
        for item in catalog:
            completed = completed_by_id.get(item["sub_task_id"])
            if completed:
                completed["catalog"] = item
                sub_results.append(completed)
            else:
                sub_results.append({
                    "id": None,
                    "attempt_id": attempt.id,
                    "sub_task_id": item["sub_task_id"],
                    "sub_task_name": item["name"],
                    "status": "pending",
                    "score": None,
                    "train_time": None,
                    "started_at": None,
                    "finished_at": None,
                    "steps": item.get("expected_steps", []),
                    "errors": [],
                    "details": None,
                    "error_count": 0,
                    "safety_error_count": 0,
                    "summary": "未完成",
                    "ai_ready": False,
                    "catalog": item,
                })
        for sub_id, completed in completed_by_id.items():
            if not any(item["sub_task_id"] == sub_id for item in catalog):
                sub_results.append(completed)
    else:
        sub_results = list(completed_by_id.values())

    if not rows:
        legacy = _legacy_sub_result(attempt)
        if legacy:
            if catalog:
                for index, item in enumerate(sub_results):
                    if item["sub_task_id"] == legacy["sub_task_id"]:
                        sub_results[index] = {**legacy, "catalog": item.get("catalog")}
                        break
                else:
                    sub_results.append(legacy)
            else:
                sub_results.append(legacy)

    done_sub_results = [item for item in sub_results if item["status"] == "done"]
    completed_count = len(done_sub_results)
    total_count = len(catalog) if catalog else len(sub_results)
    avg_score = round(sum(item["score"] or 0 for item in done_sub_results) / completed_count, 1) if completed_count else None
    error_count = sum(item["error_count"] for item in done_sub_results)
    safety_error_count = sum(item["safety_error_count"] for item in done_sub_results)

    return {
        "attempt_id": attempt.id,
        "assignment_id": attempt.assignment_id,
        "student_id": attempt.student_id,
        "username": user.username if user else None,
        "task_id": attempt.task_id,
        "task_title": task.title if task else None,
        "scene_name": attempt.scene_name,
        "status": "done" if completed_count > 0 else attempt.status,
        "score": avg_score,
        "train_time": sum(item["train_time"] or 0 for item in done_sub_results) if done_sub_results else attempt.train_time,
        "started_at": attempt.started_at,
        "finished_at": attempt.finished_at,
        "completed_sub_count": completed_count,
        "total_sub_count": total_count,
        "error_count": error_count,
        "safety_error_count": safety_error_count,
        "summary": "\n".join(item["summary"] for item in done_sub_results[:2]) if done_sub_results else "暂无已完成子项目",
        "sub_items": catalog,
        "sub_results": sub_results,
    }


def _refresh_attempt_rollup(db: Session, attempt: TrainingAttempt):
    rows = db.query(TrainingSubResult).filter(TrainingSubResult.attempt_id == attempt.id).all()
    done = [row for row in rows if row.status == "done"]
    if not done:
        return
    attempt.status = "done"
    attempt.score = round(sum(row.score or 0 for row in done) / len(done))
    attempt.train_time = sum(row.train_time or 0 for row in done)
    attempt.finished_at = datetime.utcnow()
    attempt.error_count = sum(row.error_count or 0 for row in done)
    attempt.safety_error_count = sum(row.safety_error_count or 0 for row in done)
    attempt.summary = "\n".join((row.summary or "") for row in done[:2]).strip() or "无关键错误"

    assignment = db.query(TaskAssignment).filter(TaskAssignment.id == attempt.assignment_id).first()
    if assignment:
        assignment.status = "done"
        assignment.score = attempt.score
        assignment.train_time = attempt.train_time
        assignment.finished_at = attempt.finished_at


@router.post("/start")
def start_training(data: TrainingStartRequest, db: Session = Depends(get_db)):
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == data.assignment_id,
        TaskAssignment.user_id == data.student_id,
    ).first()
    if not assignment:
        raise HTTPException(status_code=404, detail="训练分配记录不存在")

    task = db.query(Task).filter(Task.id == assignment.task_id).first()
    if not task or not is_allowed_scene(task.scene_name):
        raise HTTPException(status_code=400, detail="该训练项目暂不支持 Unity Play 联调")

    assignment.status = "running"
    attempt = TrainingAttempt(
        assignment_id=assignment.id,
        student_id=data.student_id,
        task_id=task.id,
        scene_name=task.scene_name,
        status="running",
    )
    db.add(attempt)
    db.commit()
    db.refresh(attempt)
    return _attempt_payload(db, attempt, task)


@router.post("/{attempt_id}/cancel")
def cancel_training_attempt(
    attempt_id: int,
    data: TrainingCancelRequest,
    db: Session = Depends(get_db),
):
    attempt = db.query(TrainingAttempt).filter(
        TrainingAttempt.id == attempt_id,
        TrainingAttempt.student_id == data.student_id,
    ).first()
    if not attempt:
        raise HTTPException(status_code=404, detail="Training attempt not found")

    submitted = db.query(TrainingSubResult).filter(
        TrainingSubResult.attempt_id == attempt.id
    ).first()
    if submitted or attempt.status == "done":
        raise HTTPException(status_code=400, detail="Training attempt already has results")

    attempt.status = "cancelled"
    attempt.finished_at = datetime.utcnow()
    attempt.summary = data.reason or "Cancelled before Unity launch"

    assignment = db.query(TaskAssignment).filter(TaskAssignment.id == attempt.assignment_id).first()
    if assignment and assignment.user_id == data.student_id and assignment.status == "running":
        assignment.status = "pending"
        assignment.score = None
        assignment.train_time = None
        assignment.finished_at = None

    db.commit()
    return {"success": True}


@router.get("/active")
def get_active_attempt(
    student_id: int = Query(..., gt=0),
    scene_name: str = Query(...),
    db: Session = Depends(get_db),
):
    if not is_allowed_scene(scene_name):
        raise HTTPException(status_code=400, detail="不支持的训练场景")
    attempt = db.query(TrainingAttempt).filter(
        TrainingAttempt.student_id == student_id,
        TrainingAttempt.scene_name == scene_name,
        TrainingAttempt.status.in_(["running", "done"]),
    ).order_by(TrainingAttempt.id.desc()).first()
    if not attempt:
        return {"active": False}
    task = db.query(Task).filter(Task.id == attempt.task_id).first()
    return {"active": True, **_attempt_payload(db, attempt, task)}


@router.post("/result")
def submit_training_result(data: TrainingResultSubmit, db: Session = Depends(get_db)):
    attempt = db.query(TrainingAttempt).filter(
        TrainingAttempt.id == data.attempt_id,
        TrainingAttempt.student_id == data.student_id,
    ).first()
    if not attempt:
        raise HTTPException(status_code=404, detail="训练记录不存在")
    if attempt.scene_name != data.scene_name:
        raise HTTPException(status_code=400, detail="训练场景与记录不匹配")

    sub_task_id = (data.sub_task_id or "default_sub_task").strip()
    sub_item = find_sub_item(attempt.scene_name, sub_task_id)
    sub_task_name = sub_item["name"] if sub_item else sub_task_display_name(attempt.scene_name, sub_task_id)
    safety_error_count = sum(1 for e in data.errors if e.get("severity") == "safety")
    summary = _error_summary(data.errors)
    details = data.model_dump()

    result = db.query(TrainingSubResult).filter(
        TrainingSubResult.attempt_id == attempt.id,
        TrainingSubResult.sub_task_id == sub_task_id,
    ).first()
    if not result:
        result = TrainingSubResult(
            attempt_id=attempt.id,
            student_id=data.student_id,
            task_id=attempt.task_id,
            scene_name=attempt.scene_name,
            sub_task_id=sub_task_id,
            sub_task_name=sub_task_name,
        )
        db.add(result)

    result.sub_task_name = sub_task_name
    result.status = "done"
    result.score = data.score
    result.train_time = data.train_time
    result.started_at = data.started_at
    result.finished_at = data.finished_at
    result.steps_json = json.dumps(data.steps, ensure_ascii=False)
    result.errors_json = json.dumps(data.errors, ensure_ascii=False)
    result.details_json = json.dumps(details, ensure_ascii=False)
    result.error_count = len(data.errors)
    result.safety_error_count = safety_error_count
    result.summary = summary

    db.flush()
    _refresh_attempt_rollup(db, attempt)
    db.commit()
    return {"success": True, "attempt_id": attempt.id, "sub_task_id": sub_task_id}


@router.get("/history/student/{student_id}")
def get_student_history(student_id: int, db: Session = Depends(get_db)):
    rows = (
        db.query(TrainingAttempt, Task)
        .join(Task, TrainingAttempt.task_id == Task.id)
        .filter(TrainingAttempt.student_id == student_id)
        .order_by(TrainingAttempt.id.desc())
        .all()
    )
    return [_attempt_payload(db, attempt, task) for attempt, task in rows]


@router.get("/analytics/company/{company_id}")
def get_company_analytics(company_id: int, db: Session = Depends(get_db)):
    rows = (
        db.query(TrainingAttempt, Task, User)
        .join(Task, TrainingAttempt.task_id == Task.id)
        .join(User, TrainingAttempt.student_id == User.id)
        .filter(User.company_id == company_id)
        .order_by(TrainingAttempt.id.desc())
        .all()
    )
    attempts = [_attempt_payload(db, attempt, task, user) for attempt, task, user in rows]
    done = [a for a in attempts if a["completed_sub_count"] > 0]
    avg_score = round(sum(a["score"] or 0 for a in done) / len(done), 1) if done else 0

    by_scene = []
    for scene_name, count, avg in (
        db.query(
            TrainingAttempt.scene_name,
            func.count(TrainingAttempt.id),
            func.avg(TrainingAttempt.score),
        )
        .join(User, TrainingAttempt.student_id == User.id)
        .filter(User.company_id == company_id, TrainingAttempt.status == "done")
        .group_by(TrainingAttempt.scene_name)
        .all()
    ):
        by_scene.append({
            "scene_name": scene_name,
            "completed_count": count,
            "average_score": round(float(avg or 0), 1),
        })

    return {
        "summary": {
            "attempt_count": len(attempts),
            "completed_count": len(done),
            "completed_sub_count": sum(a["completed_sub_count"] for a in attempts),
            "average_score": avg_score,
            "error_count": sum(a["error_count"] for a in attempts),
            "safety_error_count": sum(a["safety_error_count"] for a in attempts),
        },
        "by_scene": by_scene,
        "attempts": attempts,
    }
