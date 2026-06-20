"""
慧动手 —— 手势训练系统后端
FastAPI + SQLite + JWT 认证
支持三层角色：root（平台级） / admin（公司级） / student（学员）
"""
from fastapi import FastAPI, Header, HTTPException, Depends
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.orm import Session
import hashlib
from pydantic import BaseModel
from datetime import datetime

from backend.database import Base, engine, get_db
from backend.models.user import User
from backend.models.company import Company
from backend.models.task import Task
from backend.models.task_assignment import TaskAssignment
from backend.models.company_task import CompanyTask
from backend.models.training_attempt import TrainingAttempt
from backend.models.training_sub_result import TrainingSubResult
from backend.routers import task, training
from backend.auth import create_access_token, verify_token
from backend.schemas.user import UserCreate
from backend.schemas.result import ResultSubmit
from backend.schemas.login import LoginRequest
from backend.schemas.root_login import RootLoginRequest
from backend.training_catalog import is_allowed_scene


# =================================================================
# 临时 Pydantic 模型（仅用于 main.py 路由）
# =================================================================
class CompanyCreate(BaseModel):
    """创建公司请求体"""
    name: str
    code: str


class AdminCreate(BaseModel):
    """创建管理员请求体"""
    username: str
    password: str
    company_id: int


# 自动建表 —— 启动时根据 ORM 模型创建/更新数据库表结构
Base.metadata.create_all(bind=engine)

app = FastAPI(
    title="Gesture Training System",
    description="基于手势识别的工厂员工手部作业虚拟仿真培训平台",
    version="1.0.0"
)

# 注册 /task 前缀路由子模块
app.include_router(task.router)
app.include_router(training.router)

# CORS 跨域 —— 开发阶段允许所有来源
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


# =================================================================
# 工具函数
# =================================================================

def hash_password(password: str) -> str:
    """SHA256 哈希加密 —— 所有密码入库前均由此函数处理"""
    return hashlib.sha256(password.encode()).hexdigest()


# =================================================================
# 通用端点：根路径 / 测试
# =================================================================

@app.get("/")
def root():
    return {"message": "Gesture Training System Backend Running"}


@app.get("/test")
def test():
    return {"success": True, "data": "Hello From FastAPI"}


# =================================================================
# 认证模块：注册 / 登录 / Token 解析 / 当前用户
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

    # 同一公司内用户名唯一
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

    # root 无需公司校验
    if db_user.role != "root":
        if user.company_id is None:
            raise HTTPException(status_code=400, detail="必须选择公司")
        # 兼容老数据：company_id 为 NULL 时自动绑定
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


# =================================================================
# 公司列表 —— 登录页下拉选择（仅返回启用状态的公司）
# =================================================================

@app.get("/companies")
def get_companies(db: Session = Depends(get_db)):
    companies = db.query(Company).filter(Company.status == "active").all()
    return [{"id": c.id, "name": c.name, "code": c.code} for c in companies]


# =================================================================
# Root 登录
# =================================================================

@app.post("/root/login")
def root_login(data: RootLoginRequest, db: Session = Depends(get_db)):
    """Root 专用登录入口 —— 验证 root 身份并签发 JWT"""
    db_user = db.query(User).filter(
        User.username == data.username,
        User.role == "root"
    ).first()
    if not db_user or db_user.password != hash_password(data.password):
        raise HTTPException(status_code=400, detail="账号或密码错误")

    token = create_access_token({
        "user_id": db_user.id,
        "username": db_user.username,
        "role": "root",
        "company_id": None
    })
    return {
        "success": True,
        "token": token,
        "username": db_user.username,
        "user_id": db_user.id
    }


# =================================================================
# Root: 公司管理（CRUD）
# =================================================================

@app.get("/root/companies")
def get_root_companies(db: Session = Depends(get_db)):
    """Root 获取所有公司列表（含停用状态）"""
    companies = db.query(Company).order_by(Company.id.asc()).all()
    return [{"id": c.id, "name": c.name, "code": c.code, "status": c.status} for c in companies]


@app.post("/root/companies")
def create_company(data: CompanyCreate, db: Session = Depends(get_db)):
    """Root 创建公司 —— 名称和编码均需唯一"""
    if not data.name or not data.code:
        raise HTTPException(status_code=400, detail="公司名称和编码不能为空")
    if db.query(Company).filter(Company.name == data.name).first():
        raise HTTPException(status_code=400, detail="公司名称已存在")
    if db.query(Company).filter(Company.code == data.code).first():
        raise HTTPException(status_code=400, detail="公司编码已存在")

    company = Company(name=data.name, code=data.code, status="active")
    db.add(company)
    db.commit()
    return {"success": True, "message": "公司创建成功"}


@app.patch("/root/companies/{company_id}")
def update_company_status(company_id: int, data: dict, db: Session = Depends(get_db)):
    """Root 启用/停用公司"""
    company = db.query(Company).filter(Company.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="公司不存在")
    company.status = "inactive" if company.status == "active" else "active"
    db.commit()
    return {"success": True, "message": f"公司已{'停用' if company.status == 'inactive' else '启用'}"}


@app.delete("/root/companies/{company_id}")
def delete_company(company_id: int, db: Session = Depends(get_db)):
    """Root 删除公司 —— 级联删除关联用户和 CompanyTask 记录"""
    company = db.query(Company).filter(Company.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="公司不存在")
    user_ids = db.query(User.id).filter(User.company_id == company_id)
    db.query(TrainingAttempt).filter(TrainingAttempt.student_id.in_(user_ids)).delete(synchronize_session=False)
    db.query(User).filter(User.company_id == company_id).delete()
    db.query(CompanyTask).filter(CompanyTask.company_id == company_id).delete()
    db.delete(company)
    db.commit()
    return {"success": True, "message": "公司已删除"}


# =================================================================
# Root: 管理员管理
# =================================================================

@app.get("/root/admins")
def get_admins(db: Session = Depends(get_db)):
    """Root 获取管理员列表（联表查询公司名称）"""
    admins = db.query(User).filter(User.role == "admin").all()
    result = []
    for admin in admins:
        company = db.query(Company).filter(Company.id == admin.company_id).first()
        result.append({
            "id": admin.id,
            "username": admin.username,
            "company_id": admin.company_id,
            "company_name": company.name if company else "未知"
        })
    return result


@app.post("/root/admins")
def create_admin(data: AdminCreate, db: Session = Depends(get_db)):
    """Root 为公司创建管理员 —— 每公司仅允许一个管理员"""
    if not data.username or not data.password:
        raise HTTPException(status_code=400, detail="账号和密码不能为空")
    if not data.company_id:
        raise HTTPException(status_code=400, detail="必须选择所属公司")

    exist = db.query(User).filter(
        User.username == data.username,
        User.company_id == data.company_id,
        User.role == "admin"
    ).first()
    if exist:
        raise HTTPException(status_code=400, detail="该公司下管理员已存在")

    admin = User(
        username=data.username,
        password=hash_password(data.password),
        role="admin",
        company_id=data.company_id
    )
    db.add(admin)
    db.commit()
    return {"success": True, "message": "管理员创建成功"}


@app.patch("/root/admins/{admin_id}/reset-password")
def reset_admin_password(admin_id: int, db: Session = Depends(get_db)):
    """Root reset an admin password to the default value: 123."""
    admin = db.query(User).filter(User.id == admin_id, User.role == "admin").first()
    if not admin:
        raise HTTPException(status_code=404, detail="管理员不存在")
    admin.password = hash_password("123")
    db.commit()
    return {"success": True, "message": "管理员密码已重置为123"}


@app.delete("/root/admins/{admin_id}")
def delete_admin(admin_id: int, db: Session = Depends(get_db)):
    """Root 删除管理员"""
    admin = db.query(User).filter(User.id == admin_id, User.role == "admin").first()
    if not admin:
        raise HTTPException(status_code=404, detail="管理员不存在")
    db.delete(admin)
    db.commit()
    return {"success": True, "message": "管理员已删除"}


# =================================================================
# Root: 统计面板
# =================================================================

@app.get("/root/statistics")
def get_statistics(db: Session = Depends(get_db)):
    """Root 统计面板：公司数 / 管理员数 / 学员数"""
    return {
        "companies": db.query(Company).count(),
        "admins": db.query(User).filter(User.role == "admin").count(),
        "students": db.query(User).filter(User.role == "student").count()
    }


# =================================================================
# Root: 公司-训练项目关联管理
# =================================================================

@app.get("/root/company-tasks")
def get_company_task_assignments(db: Session = Depends(get_db)):
    """Root 查看所有公司-训练项目关联（联表公司名+训练名）"""
    rows = (
        db.query(CompanyTask, Company, Task)
        .join(Company, CompanyTask.company_id == Company.id)
        .join(Task, CompanyTask.task_id == Task.id)
        .filter(Task.scene_name.in_(["lead-train1", "train2"]))
        .order_by(CompanyTask.id.desc())
        .all()
    )
    return [
        {
            "id": ct.id,
            "company_id": company.id,
            "company_name": company.name,
            "task_id": task.id,
            "task_title": task.title,
            "scene_name": task.scene_name
        }
        for ct, company, task in rows
    ]


@app.post("/root/company-tasks")
def assign_task_to_company(data: dict, db: Session = Depends(get_db)):
    """Root 为某公司分配训练项目"""
    company_id = data.get("company_id")
    task_id = data.get("task_id")
    if not company_id or not task_id:
        raise HTTPException(status_code=400, detail="缺少 company_id 或 task_id")

    task_obj = db.query(Task).filter(Task.id == task_id).first()
    if not task_obj or not is_allowed_scene(task_obj.scene_name):
        raise HTTPException(status_code=400, detail="该训练项目不在当前可分配范围内")

    exist = db.query(CompanyTask).filter(
        CompanyTask.company_id == company_id,
        CompanyTask.task_id == task_id
    ).first()
    if exist:
        raise HTTPException(status_code=400, detail="该公司已拥有此训练项目")

    ct = CompanyTask(company_id=company_id, task_id=task_id)
    db.add(ct)
    db.commit()
    return {"success": True, "message": "训练项目已分配给公司"}


@app.delete("/root/company-tasks/{assignment_id}")
def remove_task_from_company(assignment_id: int, db: Session = Depends(get_db)):
    """Root 取消某公司的训练项目"""
    ct = db.query(CompanyTask).filter(CompanyTask.id == assignment_id).first()
    if not ct:
        raise HTTPException(status_code=404, detail="关联记录不存在")
    db.delete(ct)
    db.commit()
    return {"success": True, "message": "已取消分配"}


# =================================================================
# 通用端点：用户列表 / 学员任务 / 任务重新分配
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
        .filter(Task.scene_name.in_(["lead-train1", "train2"]))
        .order_by(TaskAssignment.id.desc())
        .all()
    )
    return [
        {
            "id": task.id,
            "title": task.title,
            "description": task.description,
            "scene_name": task.scene_name,
            "assignment_id": assignment.id,
            "status": assignment.status,
            "score": assignment.score,
            "train_time": assignment.train_time,
            "finished_at": assignment.finished_at
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
        task_obj = db.query(Task).filter(Task.id == data.task_id).first()
        finished_at = datetime.utcnow()
        db.add(TrainingAttempt(
            assignment_id=data.assignment_id,
            student_id=data.student_id,
            task_id=data.task_id,
            scene_name=task_obj.scene_name if task_obj else "",
            status="done",
            score=data.score,
            train_time=data.train_time,
            finished_at=finished_at,
            summary="旧接口提交结果",
        ))
        assignment.status = "done"
        assignment.score = data.score
        assignment.train_time = data.train_time
        assignment.finished_at = finished_at
        db.commit()
        print(f"训练完成: 学员={data.student_id}, 任务={data.task_id}, 成绩={data.score}, 用时={data.train_time}s")
    return {"success": True}
