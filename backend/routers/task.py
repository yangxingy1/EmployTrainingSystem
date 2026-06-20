from fastapi import APIRouter, Depends, Header, HTTPException
from sqlalchemy.orm import Session

from backend.database import get_db
from backend.models.company_task import CompanyTask
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.models.user import User
from backend.schemas.assignment import AssignmentCreate
from backend.schemas.task import TaskCreate
from backend.auth import verify_token
from backend.training_catalog import is_allowed_scene, normalize_scene_name

router = APIRouter(prefix="/task", tags=["Task"])

ALLOWED_SCENE_LIST = ["lead-train1", "train2"]


def _task_payload(task: Task):
    return {
        "id": task.id,
        "title": task.title,
        "description": task.description,
        "scene_name": task.scene_name,
    }


def _require_allowed_task(db: Session, task_id: int) -> Task:
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    if not is_allowed_scene(task.scene_name):
        raise HTTPException(status_code=400, detail="该训练项目不在当前可分配范围内")
    return task


def _require_current_user(authorization: str = Header(None)) -> dict:
    if not authorization:
        raise HTTPException(status_code=401, detail="未登录")
    token = authorization.replace("Bearer ", "")
    payload = verify_token(token)
    if not payload:
        raise HTTPException(status_code=401, detail="Token失效")
    return payload


@router.post("/create")
def create_task(task: TaskCreate, db: Session = Depends(get_db)):
    if not task.title or not task.title.strip():
        raise HTTPException(status_code=400, detail="训练名称不能为空")
    scene_name = normalize_scene_name(task.scene_name)
    if not is_allowed_scene(scene_name):
        raise HTTPException(status_code=400, detail="当前只允许创建 lead-train1 或 train2")
    if db.query(Task).filter(Task.scene_name == scene_name).first():
        raise HTTPException(status_code=400, detail="该 Unity 场景已存在训练项目")

    new_task = Task(title=task.title.strip(), description=task.description, scene_name=scene_name)
    db.add(new_task)
    db.commit()
    db.refresh(new_task)
    return {"success": True, "task_id": new_task.id}


@router.get("/list")
def get_tasks(db: Session = Depends(get_db)):
    tasks = (
        db.query(Task)
        .filter(Task.scene_name.in_(ALLOWED_SCENE_LIST))
        .order_by(Task.id.desc())
        .all()
    )
    return [_task_payload(t) for t in tasks]


@router.get("/student-access/{student_id}")
def get_student_training_access(student_id: int, db: Session = Depends(get_db)):
    student = db.query(User).filter(User.id == student_id, User.role == "student").first()
    if not student:
        raise HTTPException(status_code=404, detail="student not found")

    tasks_by_scene = {
        task.scene_name: task
        for task in db.query(Task).filter(Task.scene_name.in_(ALLOWED_SCENE_LIST)).all()
    }

    payload = []
    for scene_name in ALLOWED_SCENE_LIST:
        task = tasks_by_scene.get(scene_name)
        company_link = None
        assignment = None

        if task and student.company_id:
            company_link = db.query(CompanyTask).filter(
                CompanyTask.company_id == student.company_id,
                CompanyTask.task_id == task.id,
            ).first()
            assignment = db.query(TaskAssignment).filter(
                TaskAssignment.user_id == student.id,
                TaskAssignment.task_id == task.id,
            ).order_by(TaskAssignment.id.desc()).first()

        payload.append({
            "scene_name": scene_name,
            "task_id": task.id if task else None,
            "title": task.title if task else scene_name,
            "description": task.description if task else "",
            "unlocked": company_link is not None,
            "assigned": assignment is not None,
            "assignment_id": assignment.id if assignment else None,
            "status": assignment.status if assignment else "",
        })

    return {
        "student_id": student.id,
        "company_id": student.company_id,
        "items": payload,
    }


@router.put("/{task_id}")
def update_task(task_id: int, data: dict, db: Session = Depends(get_db)):
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    if "title" in data and data["title"].strip():
        task.title = data["title"].strip()
    if "description" in data:
        task.description = data["description"]
    if "scene_name" in data:
        scene_name = normalize_scene_name(data["scene_name"])
        if not is_allowed_scene(scene_name):
            raise HTTPException(status_code=400, detail="当前只允许创建 lead-train1 或 train2")
        existing = db.query(Task).filter(Task.scene_name == scene_name, Task.id != task_id).first()
        if existing:
            raise HTTPException(status_code=400, detail="该 Unity 场景已存在训练项目")
        task.scene_name = scene_name
    db.commit()
    return {"success": True, "message": "训练项目已更新"}


@router.delete("/{task_id}")
def delete_task(task_id: int, db: Session = Depends(get_db)):
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    db.query(CompanyTask).filter(CompanyTask.task_id == task_id).delete()
    db.query(TaskAssignment).filter(TaskAssignment.task_id == task_id).delete()
    db.delete(task)
    db.commit()
    return {"success": True, "message": "训练项目已删除"}


@router.get("/company/{company_id}")
def get_company_tasks(company_id: int, db: Session = Depends(get_db)):
    rows = (
        db.query(Task)
        .join(CompanyTask, CompanyTask.task_id == Task.id)
        .filter(CompanyTask.company_id == company_id)
        .filter(Task.scene_name.in_(ALLOWED_SCENE_LIST))
        .order_by(Task.id.desc())
        .all()
    )
    return [_task_payload(t) for t in rows]


@router.post("/company/{company_id}/add")
def add_task_to_company(company_id: int, data: dict, db: Session = Depends(get_db)):
    task_id = data.get("task_id")
    if not task_id:
        raise HTTPException(status_code=400, detail="缺少 task_id")
    task = _require_allowed_task(db, task_id)
    exist = db.query(CompanyTask).filter(
        CompanyTask.company_id == company_id,
        CompanyTask.task_id == task_id,
    ).first()
    if exist:
        raise HTTPException(status_code=400, detail="该公司已拥有此训练项目")
    db.add(CompanyTask(company_id=company_id, task_id=task_id))
    db.commit()
    return {"success": True, "message": f"已添加训练项目: {task.title}"}


@router.get("/global/list")
def get_global_tasks(db: Session = Depends(get_db)):
    tasks = (
        db.query(Task)
        .filter(Task.scene_name.in_(ALLOWED_SCENE_LIST))
        .order_by(Task.id.desc())
        .all()
    )
    return [_task_payload(t) for t in tasks]


@router.get("/assignments")
def get_assignments(payload: dict = Depends(_require_current_user), db: Session = Depends(get_db)):
    query = (
        db.query(TaskAssignment, User, Task)
        .join(User, TaskAssignment.user_id == User.id)
        .join(Task, TaskAssignment.task_id == Task.id)
        .filter(Task.scene_name.in_(ALLOWED_SCENE_LIST))
    )
    if payload.get("role") == "admin":
        query = query.filter(User.company_id == payload.get("company_id"))
    elif payload.get("role") == "student":
        query = query.filter(User.id == payload.get("user_id"))

    rows = query.order_by(TaskAssignment.id.desc()).all()
    return [
        {
            "id": assignment.id,
            "user_id": user.id,
            "username": user.username,
            "task_id": task.id,
            "task_title": task.title,
            "task_description": task.description,
            "scene_name": task.scene_name,
            "status": assignment.status,
        }
        for assignment, user, task in rows
    ]


@router.post("/assign")
def assign_task(data: AssignmentCreate, payload: dict = Depends(_require_current_user), db: Session = Depends(get_db)):
    if not data.user_id or not data.task_id:
        raise HTTPException(status_code=400, detail="缺少必要参数")
    _require_allowed_task(db, data.task_id)

    if payload.get("role") != "admin":
        raise HTTPException(status_code=403, detail="仅管理员可分配训练")
    company_id = payload.get("company_id")
    student = db.query(User).filter(
        User.id == data.user_id,
        User.role == "student",
        User.company_id == company_id,
    ).first()
    if not student:
        raise HTTPException(status_code=403, detail="只能给本公司学员分配训练")
    company_task = db.query(CompanyTask).filter(
        CompanyTask.company_id == company_id,
        CompanyTask.task_id == data.task_id,
    ).first()
    if not company_task:
        raise HTTPException(status_code=403, detail="只能分配本公司已开通的训练")

    existing = db.query(TaskAssignment).filter(
        TaskAssignment.user_id == data.user_id,
        TaskAssignment.task_id == data.task_id,
    ).first()
    if existing:
        return {"success": True, "assignment_id": existing.id, "existing": True}

    assignment = TaskAssignment(user_id=data.user_id, task_id=data.task_id, status="pending")
    db.add(assignment)
    db.commit()
    db.refresh(assignment)
    return {"success": True, "assignment_id": assignment.id}


@router.get("/launcher/task/{user_id}")
def get_launcher_task(user_id: int, db: Session = Depends(get_db)):
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.user_id == user_id,
        TaskAssignment.status == "pending",
    ).first()
    if assignment:
        assignment.status = "running"
        db.commit()
        task = db.query(Task).filter(Task.id == assignment.task_id).first()
        return {
            "start_training": True,
            "assignment_id": assignment.id,
            "task_id": task.id if task else assignment.task_id,
            "scene_name": task.scene_name if task else None,
        }
    return {"start_training": False}


@router.post("/finish")
def finish_task(data: dict, db: Session = Depends(get_db)):
    assignment = db.query(TaskAssignment).filter(TaskAssignment.id == data["assignment_id"]).first()
    if assignment:
        assignment.status = "done"
        db.commit()
        return {"success": True}
    raise HTTPException(status_code=404, detail="分配记录不存在")
