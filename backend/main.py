"""
慧动手 —— 手势训练系统后端
FastAPI + SQLite + JWT 认证
支持三层角色：root（平台级） / admin（公司级） / student（学员）
"""
from fastapi import FastAPI, Header, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.orm import Session
from pydantic import BaseModel
from datetime import datetime

import os
from dotenv import load_dotenv
load_dotenv(os.path.join(os.path.dirname(os.path.abspath(__file__)), '.env'))

from backend.database import Base, engine, get_db
from backend.models.user import User
from backend.models.company import Company
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.models.company_task import CompanyTask
from backend.routers import task, root
from backend.auth import create_access_token, verify_token, hash_password
from backend.schemas.user import UserCreate
from backend.schemas.result import ResultSubmit
from backend.schemas.login import LoginRequest


# 自动建表 —— 启动时根据 ORM 模型创建/更新数据库表结构
Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="Gesture Training System",
    description="基于手势识别的工厂员工手部作业虚拟仿真培训平台",
    version="1.0.0"
)

# 注册路由子模块
app.include_router(task.router)
app.include_router(root.router)

# CORS 跨域 —— 生产环境通过 .env 中 CORS_ORIGINS 配置域名
app.add_middleware(
    CORSMiddleware,
    allow_origins=os.getenv("CORS_ORIGINS", "*").split(","),
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# =================================================================
# 通用端点：根路径 / 测试
# =================================================================

@app.get("/")
def root_endpoint():
    return {"message": "Gesture Training System Backend Running"}


@app.get("/test")
def test():
    return {"success": True, "data": "Hello From FastAPI"}


# =================================================================
# 认证模块：注册 / 登录 / 当前用户 / 公司列表
# =================================================================

@app.post("/register")
def register(user: UserCreate, db: Session = Depends(get_db)):
    """
    学员注册
    校验 username/password/role/company_id -> 公司内查重 -> 哈希密码 -> 入库
    """
    if not user.username or not user.username.strip():
        raise HTTPException(status_code=400, detail="用户名不能为空")
    if not user.password or len(user.password) < 3:
        raise HTTPException(status_code=400, detail="密码至少需要3位")
    if user.role != "student":
        raise HTTPException(status_code=400, detail="仅支持学员注册")
    if not user.company_id:
        raise HTTPException(status_code=400, detail="必须选择所属公司")

    existing = db.query(User).filter(
        User.username == user.username,
        User.company_id == user.company_id
    ).first()
    if existing:
        raise HTTPException(status_code=400, detail="该学员已在当前公司注册")

    new_user = User(
        username=user.username.strip(),
        password=hash_password(user.password),
        role="student",
        company_id=user.company_id
    )
    db.add(new_user)
    db.commit()
    db.refresh(new_user)
    return {"success": True, "message": "注册成功", "user_id": new_user.id}


@app.get("/me")
def get_me(authorization: str = Header(None)):
    """从 Authorization 请求头解析当前用户信息（JWT payload）"""
    if not authorization:
        raise HTTPException(status_code=401, detail="未登录")
    token = authorization.replace("Bearer ", "")
    payload = verify_token(token)
    if not payload:
        raise HTTPException(status_code=401, detail="Token失效")
    return payload


@app.post("/login")
def login(user: LoginRequest, db: Session = Depends(get_db)):
    """
    统一登录入口
    - root: 跳过公司校验，直接签发 Token
    - admin / student: 必须提供 company_id 并校验匹配
    """
    if not user.username or not user.password:
        raise HTTPException(status_code=400, detail="用户名和密码不能为空")

    db_user = db.query(User).filter(User.username == user.username).first()
    if not db_user:
        raise HTTPException(status_code=400, detail="用户不存在")

    if db_user.password != hash_password(user.password):
        raise HTTPException(status_code=400, detail="账号或密码错误")

    if db_user.role != "root":
        if user.company_id is None:
            raise HTTPException(status_code=400, detail="必须选择公司")
        if db_user.company_id is None:
            db_user.company_id = user.company_id
            db.commit()
        elif db_user.company_id != user.company_id:
            raise HTTPException(status_code=400, detail="公司不匹配，请检查选择")

    token = create_access_token({
        "user_id": db_user.id,
        "username": db_user.username,
        "role": db_user.role,
        "company_id": db_user.company_id
    })
    return {
        "success": True,
        "token": token,
        "username": db_user.username,
        "role": db_user.role,
        "user_id": db_user.id,
        "company_id": db_user.company_id
    }


@app.get("/companies")
def get_companies(db: Session = Depends(get_db)):
    """公司列表 —— 登录页下拉选择（仅返回启用状态的公司）"""
    companies = db.query(Company).filter(Company.status == "active").all()
    return [{"id": c.id, "name": c.name, "code": c.code} for c in companies]


# =================================================================
# 通用端点：用户列表 / 学员任务 / 任务重新分配 / 学员删除
# =================================================================

@app.get("/users")
def get_users(db: Session = Depends(get_db)):
    """获取所有用户列表 —— Admin 前端按 company_id 过滤"""
    users = db.query(User).order_by(User.id.asc()).all()
    return [{"id": u.id, "username": u.username, "role": u.role, "company_id": u.company_id} for u in users]


@app.get("/my-tasks/{user_id}")
def get_my_tasks(user_id: int, db: Session = Depends(get_db)):
    """学员查看自己分配到的训练任务 —— 联表 Task + TaskAssignment"""
    rows = (
        db.query(Task, TaskAssignment)
        .join(TaskAssignment, TaskAssignment.task_id == Task.id)
        .filter(TaskAssignment.user_id == user_id)
        .order_by(TaskAssignment.id.desc())
        .all()
    )
    return [
        {
            "id": task.id,
            "title": task.title,
            "description": task.description,
            "assignment_id": assignment.id,
            "status": assignment.status
        }
        for task, assignment in rows
    ]


@app.post("/task/reassign/{assignment_id}")
def reassign_task(assignment_id: int, db: Session = Depends(get_db)):
    """将已完成的分配重置为 pending，允许学员重新训练"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == assignment_id
    ).first()
    if not assignment:
        raise HTTPException(status_code=404, detail="分配记录不存在")
    if assignment.status != "done":
        raise HTTPException(status_code=400, detail="只能重新分配已完成的任务")

    assignment.status = "pending"
    db.commit()
    return {"success": True, "message": "任务已重新分配"}


@app.delete("/users/{user_id}")
def delete_student(user_id: int, db: Session = Depends(get_db)):
    """管理员删除本公司学员（同时清理其任务分配记录）"""
    student = db.query(User).filter(User.id == user_id, User.role == "student").first()
    if not student:
        raise HTTPException(status_code=404, detail="学员不存在")

    db.query(TaskAssignment).filter(TaskAssignment.user_id == user_id).delete()
    db.delete(student)
    db.commit()
    return {"success": True, "message": f"学员 {student.username} 已删除"}


# =================================================================
# Unity 启动器接口（旧版兼容）
# =================================================================

@app.get("/task/pull/{device_id}")
def pull_task(device_id: str, db: Session = Depends(get_db)):
    """Unity 客户端拉取待执行任务，标记为 running"""
    task = db.query(TaskAssignment).filter(
        TaskAssignment.status.in_(["pending", "未开始"])
    ).first()
    if not task:
        return {}
    task.status = "running"
    db.commit()
    return {"task_id": task.task_id, "user_id": task.user_id}


@app.post("/task/report")
def report_task(data: dict, db: Session = Depends(get_db)):
    """Unity 客户端上报任务完成"""
    task_id = data.get("task_id")
    user_id = data.get("user_id")
    if not task_id or not user_id:
        raise HTTPException(status_code=400, detail="缺少必要参数")

    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.task_id == task_id,
        TaskAssignment.user_id == user_id
    ).first()
    if assignment:
        assignment.status = "done"
        db.commit()
    return {"success": True}


@app.post("/submit_result")
def submit_result(data: ResultSubmit, db: Session = Depends(get_db)):
    """接收 Unity 训练结果：成绩/用时入库并标记完成"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == data.assignment_id
    ).first()
    if assignment:
        assignment.status = "done"
        assignment.score = data.score
        assignment.train_time = data.train_time
        assignment.finished_at = datetime.utcnow()
        db.commit()
        print(f"训练完成: 学员={data.student_id}, 任务={data.task_id}, 成绩={data.score}, 用时={data.train_time}s")
    return {"success": True}
