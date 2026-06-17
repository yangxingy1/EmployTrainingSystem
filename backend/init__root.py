# Root 账号初始化脚本 —— 首次运行时自动创建 root 平台管理员
import os
from dotenv import load_dotenv
from sqlalchemy.orm import Session
from backend.database import SessionLocal
from backend.models.user import User
import hashlib

load_dotenv(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env"))

# root 默认凭证 —— 生产环境应修改
ROOT_USERNAME = os.getenv("ROOT_USERNAME")
ROOT_PASSWORD = os.getenv("ROOT_PASSWORD")


def hash_password(password: str):
    """SHA256 哈希"""
    return hashlib.sha256(password.encode()).hexdigest()


def create_root():
    """检查 root 账号是否存在, 不存在则创建默认账号"""
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
