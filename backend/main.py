"""
慧动手 —— 手势训练系统后端
FastAPI + SQLite + JWT 认证
"""
from fastapi import FastAPI, Header, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.orm import Session
import hashlib

from backend.database import Base, engine, get_db
from backend.schemas.user import UserCreate
from backend.models.user import User
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.routers import task
from backend.auth import create_access_token, verify_token
from backend.schemas.result import ResultSubmit

# 自动建表（表不存在时创建）
Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="Gesture Training System",
    description="基于手势识别的工厂员工手部作业虚拟仿真培训平台",
    version="1.0.0"
)

# 注册任务路由
app.include_router(task.router)

# CORS 跨域（开发阶段允许所有来源）
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


def hash_password(password: str) -> str:
    """SHA256 哈希加密密码"""
    return hashlib.sha256(password.encode()).hexdigest()


@app.get("/")
def root():
    return {"message": "Gesture Training System Backend Running"}


@app.get("/test")
def test():
    return {"success": True, "data": "Hello From FastAPI"}


# ============ 认证模块 ============

@app.post("/register")
def register(user: UserCreate, db: Session = Depends(get_db)):
    """用户注册：校验字段 -> 查重 -> 哈希密码 -> 入库"""
    if not user.username or not user.username.strip():
        raise HTTPException(status_code=400, detail="用户名不能为空")
    if not user.password or len(user.password) < 3:
        raise HTTPException(status_code=400, detail="密码至少需要3位")
    if user.role not in ("student", "admin"):
        raise HTTPException(status_code=400, detail="无效的角色类型")

    existing = db.query(User).filter(User.username == user.username).first()
    if existing:
        raise HTTPException(status_code=400, detail="用户名已存在")

    new_user = User(
        username=user.username.strip(),
        password=hash_password(user.password),
        role=user.role
    )
    db.add(new_user)
    db.commit()
    db.refresh(new_user)
    return {"success": True, "message": "注册成功", "user_id": new_user.id}


@app.get("/me")
def get_me(authorization: str = Header(None)):
    """从请求头 Token 解析当前用户信息"""
    if not authorization:
        raise HTTPException(status_code=401, detail="未登录")
    token = authorization.replace("Bearer ", "")
    payload = verify_token(token)
    if not payload:
        raise HTTPException(status_code=401, detail="Token失效")
    return payload


@app.post("/login")
def login(user: UserCreate, db: Session = Depends(get_db)):
    """登录：验证用户名密码 -> 签发 JWT Token"""
    if not user.username or not user.password:
        raise HTTPException(status_code=400, detail="用户名和密码不能为空")

    db_user = db.query(User).filter(User.username == user.username).first()
    if not db_user:
        raise HTTPException(status_code=400, detail="用户不存在")
    if db_user.password != hash_password(user.password):
        raise HTTPException(status_code=400, detail="账号或密码错误")

    token = create_access_token({
        "user_id": db_user.id,
        "username": db_user.username,
        "role": db_user.role
    })
    return {
        "success": True, "message": "登录成功",
        "token": token, "user_id": db_user.id,
        "username": db_user.username, "role": db_user.role
    }


# ============ 用户与任务 ============

@app.get("/users")
def get_users(db: Session = Depends(get_db)):
    """获取所有用户列表（管理员用）"""
    users = db.query(User).order_by(User.id.asc()).all()
    return [{"id": u.id, "username": u.username, "role": u.role} for u in users]


@app.get("/my-tasks/{user_id}")
def get_my_tasks(user_id: int, db: Session = Depends(get_db)):
    """学员查看自己分配到的训练任务"""
    rows = (
        db.query(Task, TaskAssignment)
        .join(TaskAssignment)
        .filter(TaskAssignment.user_id == user_id)
        .order_by(TaskAssignment.id.desc())
        .all()
    )
    return [
        {
            "id": task.id, "title": task.title,
            "description": task.description,
            "assignment_id": assignment.id, "status": assignment.status
        }
        for task, assignment in rows
    ]


# ============ Unity 启动器接口 ============

@app.get("/task/pull/{device_id}")
def pull_task(device_id: str, db: Session = Depends(get_db)):
    """Unity 客户端拉取待执行任务，自动标记为 running"""
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

    task = db.query(TaskAssignment).filter(
        TaskAssignment.task_id == task_id,
        TaskAssignment.user_id == user_id
    ).first()
    if task:
        task.status = "done"
        db.commit()
    return {"success": True}


# ============ 训练结果提交 ============

@app.post("/submit_result")
def submit_result(
        data: ResultSubmit,
        db: Session = Depends(get_db)
):
    """接收 Unity 训练结果：成绩、用时，并标记任务完成"""
    assignment = db.query(TaskAssignment).filter(
        TaskAssignment.id == data.assignment_id
    ).first()

    if assignment:
        assignment.status = "done"
        db.commit()
        print(f"训练完成: 学员={data.student_id}, 任务={data.task_id}, 成绩={data.score}, 用时={data.train_time}s")

    return {"success": True}
