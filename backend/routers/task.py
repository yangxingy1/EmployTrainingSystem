# 任务相关 API 路由
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from backend.database import get_db
from backend.models.user import User
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.schemas.task import TaskCreate
from backend.schemas.assignment import AssignmentCreate

router = APIRouter(prefix="/task", tags=["Task"])


@router.post("/create")
def create_task(task: TaskCreate, db: Session = Depends(get_db)):
    """管理员创建训练项目"""
    if not task.title or not task.title.strip():
        raise HTTPException(status_code=400, detail="训练名称不能为空")

    new_task = Task(title=task.title.strip(), description=task.description)
    db.add(new_task)
    db.commit()
    db.refresh(new_task)
    return {"success": True, "task_id": new_task.id}


@router.get("/list")
def get_tasks(db: Session = Depends(get_db)):
    """获取所有训练项目列表（按创建时间倒序）"""
    return db.query(Task).order_by(Task.id.desc()).all()


@router.get("/assignments")
def get_assignments(db: Session = Depends(get_db)):
    """获取所有分配记录（联表查询学员 + 训练信息）"""
    rows = (
        db.query(TaskAssignment, User, Task)
        .join(User, TaskAssignment.user_id == User.id)
        .join(Task, TaskAssignment.task_id == Task.id)
        .order_by(TaskAssignment.id.desc())
        .all()
    )
    return [
        {
            "id": assignment.id,
            "user_id": user.id,
            "username": user.username,
            "task_id": task.id,
            "task_title": task.title,
            "task_description": task.description,
            "status": assignment.status
        }
        for assignment, user, task in rows
    ]


@router.post("/assign")
def assign_task(data: AssignmentCreate, db: Session = Depends(get_db)):
    """为学员分配训练任务，已存在则跳过"""
    if not data.user_id or not data.task_id:
        raise HTTPException(status_code=400, detail="缺少必要参数")

    existing = db.query(TaskAssignment).filter(
        TaskAssignment.user_id == data.user_id,
        TaskAssignment.task_id == data.task_id
    ).first()

    if existing:
        if existing.status == "未开始":
            existing.status = "pending"
            db.commit()
        return {"success": True, "assignment_id": existing.id, "existing": True}

    assignment = TaskAssignment(
        user_id=data.user_id,
        task_id=data.task_id,
        status="pending"
    )
    db.add(assignment)
    db.commit()
    db.refresh(assignment)
    return {"success": True, "assignment_id": assignment.id}


# ============ Launcher 轮询接口 ============

@router.get("/launcher/task/{user_id}")
def get_launcher_task(user_id: int, db: Session = Depends(get_db)):
    """Launcher 轮询：查找 pending 任务 -> 标记 running -> 返回任务信息"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.user_id == user_id,
        TaskAssignment.status == "pending"
    ).first()

    if assignment:
        assignment.status = "running"
        db.commit()

        task = db.query(Task).filter(Task.id == assignment.task_id).first()
        return {
            "start_training": True,
            "assignment_id": assignment.id,
            "task_id": task.id if task else assignment.task_id
        }

    return {"start_training": False}


@router.post("/finish")
def finish_task(data: dict, db: Session = Depends(get_db)):
    """Launcher 通知：训练完成，标记 done"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == data["assignment_id"]
    ).first()

    if assignment:
        assignment.status = "done"
        db.commit()
        return {"success": True}

    raise HTTPException(status_code=404, detail="分配记录不存在")
