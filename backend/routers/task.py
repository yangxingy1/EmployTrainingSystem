# 任务相关 API 路由 —— Root 可全局 CRUD，Admin 查本公司项目
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from backend.database import get_db
from backend.models.user import User
from backend.models.task import Task
from backend.models.company_task import CompanyTask
from backend.models.task_assignment import TaskAssignment
from backend.schemas.task import TaskCreate
from backend.schemas.assignment import AssignmentCreate

router = APIRouter(prefix="/task", tags=["Task"])


# ============ 训练项目 CRUD ============

@router.post("/create")
def create_task(task: TaskCreate, db: Session = Depends(get_db)):
    """Root 创建全局训练项目"""
    if not task.title or not task.title.strip():
        raise HTTPException(status_code=400, detail="训练名称不能为空")

    new_task = Task(title=task.title.strip(), description=task.description)
    db.add(new_task)
    db.commit()
    db.refresh(new_task)
    return {"success": True, "task_id": new_task.id}


@router.get("/list")
def get_tasks(db: Session = Depends(get_db)):
    """获取所有训练项目列表（按 ID 倒序）"""
    return db.query(Task).order_by(Task.id.desc()).all()


@router.put("/{task_id}")
def update_task(task_id: int, data: dict, db: Session = Depends(get_db)):
    """Root 修改训练项目名称和说明"""
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    if "title" in data and data["title"].strip():
        task.title = data["title"].strip()
    if "description" in data:
        task.description = data["description"]
    db.commit()
    return {"success": True, "message": "训练项目已更新"}


@router.delete("/{task_id}")
def delete_task(task_id: int, db: Session = Depends(get_db)):
    """Root 删除训练项目（同时删除关联分配和公司绑定）"""
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    # 级联删除：company_tasks + task_assignments
    db.query(CompanyTask).filter(CompanyTask.task_id == task_id).delete()
    db.query(TaskAssignment).filter(TaskAssignment.task_id == task_id).delete()
    db.delete(task)
    db.commit()
    return {"success": True, "message": "训练项目已删除"}


# ============ 公司-训练项目关联（Root） ============

@router.get("/company/{company_id}")
def get_company_tasks(company_id: int, db: Session = Depends(get_db)):
    """获取某公司可用的训练项目列表（Admin 下拉用）"""
    rows = (
        db.query(Task)
        .join(CompanyTask, CompanyTask.task_id == Task.id)
        .filter(CompanyTask.company_id == company_id)
        .order_by(Task.id.desc())
        .all()
    )
    return [{"id": t.id, "title": t.title, "description": t.description} for t in rows]


@router.post("/company/{company_id}/add")
def add_task_to_company(company_id: int, data: dict, db: Session = Depends(get_db)):
    """管理员从总库添加训练项目到本公司"""
    task_id = data.get("task_id")
    if not task_id:
        raise HTTPException(status_code=400, detail="缺少 task_id")
    task = db.query(Task).filter(Task.id == task_id).first()
    if not task:
        raise HTTPException(status_code=404, detail="训练项目不存在")
    exist = db.query(CompanyTask).filter(
        CompanyTask.company_id == company_id,
        CompanyTask.task_id == task_id
    ).first()
    if exist:
        raise HTTPException(status_code=400, detail="该公司已拥有此训练项目")
    ct = CompanyTask(company_id=company_id, task_id=task_id)
    db.add(ct)
    db.commit()
    return {"success": True, "message": f"已添加训练项目: {task.title}"}


@router.get("/global/list")
def get_global_tasks(db: Session = Depends(get_db)):
    """获取全局训练项目总库（供管理员浏览选择）"""
    tasks = db.query(Task).order_by(Task.id.desc()).all()
    return [{"id": t.id, "title": t.title, "description": t.description} for t in tasks]


# ============ 分配记录查询 ============

@router.get("/assignments")
def get_assignments(company_id: int = None, db: Session = Depends(get_db)):
    """获取分配记录 —— 可按公司ID过滤，联表查询学员 + 训练信息"""
    query = (
        db.query(TaskAssignment, User, Task)
        .join(User, TaskAssignment.user_id == User.id)
        .join(Task, TaskAssignment.task_id == Task.id)
    )
    if company_id is not None:
        query = query.filter(User.company_id == company_id)
    rows = query.order_by(TaskAssignment.id.desc()).all()
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
    """为学员分配训练任务 —— 已存在则跳过并修正状态"""
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
    """Launcher 轮询: 查找 pending 任务 -> 标记 running -> 返回任务信息"""
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
    """Launcher 通知: 训练完成，标记 done"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == data["assignment_id"]
    ).first()

    if assignment:
        assignment.status = "done"
        db.commit()
        return {"success": True}

    raise HTTPException(status_code=404, detail="分配记录不存在")
