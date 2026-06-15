# Root 管理员路由 —— 平台级：公司管理 / 管理员管理 / 统计 / 训练项目分配
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session

from backend.database import get_db
from backend.models.user import User
from backend.models.company import Company
from backend.models.task import Task
from backend.models.company_task import CompanyTask
from backend.auth import create_access_token, hash_password
from backend.schemas.company import CompanyCreate, AdminCreate
from backend.schemas.root_login import RootLoginRequest

router = APIRouter(prefix="/root", tags=["Root"])


# =================================================================
# Root 登录
# =================================================================

@router.post("/login")
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
# 公司管理（CRUD）
# =================================================================

@router.get("/companies")
def get_root_companies(db: Session = Depends(get_db)):
    """Root 获取所有公司列表（含停用状态）"""
    companies = db.query(Company).order_by(Company.id.asc()).all()
    return [{"id": c.id, "name": c.name, "code": c.code, "status": c.status} for c in companies]


@router.post("/companies")
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


@router.patch("/companies/{company_id}")
def update_company_status(company_id: int, data: dict, db: Session = Depends(get_db)):
    """Root 启用/停用公司"""
    company = db.query(Company).filter(Company.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="公司不存在")
    company.status = "inactive" if company.status == "active" else "active"
    db.commit()
    return {"success": True, "message": f"公司已{'停用' if company.status == 'inactive' else '启用'}"}


@router.delete("/companies/{company_id}")
def delete_company(company_id: int, db: Session = Depends(get_db)):
    """Root 删除公司 —— 级联删除关联用户和 CompanyTask 记录"""
    company = db.query(Company).filter(Company.id == company_id).first()
    if not company:
        raise HTTPException(status_code=404, detail="公司不存在")
    db.query(User).filter(User.company_id == company_id).delete()
    db.query(CompanyTask).filter(CompanyTask.company_id == company_id).delete()
    db.delete(company)
    db.commit()
    return {"success": True, "message": "公司已删除"}


# =================================================================
# 管理员管理
# =================================================================

@router.get("/admins")
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


@router.post("/admins")
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


@router.delete("/admins/{admin_id}")
def delete_admin(admin_id: int, db: Session = Depends(get_db)):
    """Root 删除管理员"""
    admin = db.query(User).filter(User.id == admin_id, User.role == "admin").first()
    if not admin:
        raise HTTPException(status_code=404, detail="管理员不存在")
    db.delete(admin)
    db.commit()
    return {"success": True, "message": "管理员已删除"}


# =================================================================
# 统计面板
# =================================================================

@router.get("/statistics")
def get_statistics(db: Session = Depends(get_db)):
    """Root 统计面板：公司数 / 管理员数 / 学员数"""
    return {
        "companies": db.query(Company).count(),
        "admins": db.query(User).filter(User.role == "admin").count(),
        "students": db.query(User).filter(User.role == "student").count()
    }


# =================================================================
# 公司-训练项目关联管理
# =================================================================

@router.get("/company-tasks")
def get_company_task_assignments(db: Session = Depends(get_db)):
    """Root 查看所有公司-训练项目关联（联表公司名+训练名）"""
    rows = (
        db.query(CompanyTask, Company, Task)
        .join(Company, CompanyTask.company_id == Company.id)
        .join(Task, CompanyTask.task_id == Task.id)
        .order_by(CompanyTask.id.desc())
        .all()
    )
    return [
        {
            "id": ct.id,
            "company_id": company.id,
            "company_name": company.name,
            "task_id": task.id,
            "task_title": task.title
        }
        for ct, company, task in rows
    ]


@router.post("/company-tasks")
def assign_task_to_company(data: dict, db: Session = Depends(get_db)):
    """Root 为某公司分配训练项目"""
    company_id = data.get("company_id")
    task_id = data.get("task_id")
    if not company_id or not task_id:
        raise HTTPException(status_code=400, detail="缺少 company_id 或 task_id")

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


@router.delete("/company-tasks/{assignment_id}")
def remove_task_from_company(assignment_id: int, db: Session = Depends(get_db)):
    """Root 取消某公司的训练项目"""
    ct = db.query(CompanyTask).filter(CompanyTask.id == assignment_id).first()
    if not ct:
        raise HTTPException(status_code=404, detail="关联记录不存在")
    db.delete(ct)
    db.commit()
    return {"success": True, "message": "已取消分配"}
