# Root 账号初始化脚本 —— 首次部署时创建 root 平台管理员
# 运行方式: python -m backend.init__root（在项目根目录执行）
import os
import sys

# 确保项目根目录在 Python 搜索路径中（兼容从任意目录运行）
_PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
if _PROJECT_ROOT not in sys.path:
    sys.path.insert(0, _PROJECT_ROOT)

from dotenv import load_dotenv
from sqlalchemy.orm import Session
from backend.database import SessionLocal
from backend.models.user import User
import hashlib

# 指定路径，兼容服务器部署时 CWD 不在 backend/ 的情况
load_dotenv(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env"))

ROOT_USERNAME = os.getenv("ROOT_USERNAME")
ROOT_PASSWORD = os.getenv("ROOT_PASSWORD")


def hash_password(password: str):
    """SHA256 哈希加密"""
    return hashlib.sha256(password.encode()).hexdigest()


def create_root():
    """检查 root 账号是否存在，不存在则创建默认账号"""
    db: Session = SessionLocal()
    try:
        root = db.query(User).filter(User.role == "root").first()
        if root:
            print("Root 账号已存在")
            return

        root_user = User(
            username=ROOT_USERNAME,
            password=hash_password(ROOT_PASSWORD),
            role="root",
            company_id=None  # root 不属于任何公司
        )
        db.add(root_user)
        db.commit()
        print("Root 账号创建成功")
        print("账号:", ROOT_USERNAME)
        print("密码:", ROOT_PASSWORD)
    finally:
        db.close()


if __name__ == "__main__":
    create_root()