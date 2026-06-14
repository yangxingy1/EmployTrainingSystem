# 用户模型 —— root / admin / student，按公司隔离
from sqlalchemy import Column, Integer, String, DateTime, ForeignKey
from sqlalchemy.sql import func
from backend.database import Base


class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, index=True)
    username = Column(String, nullable=False)               # 同公司内唯一
    password = Column(String, nullable=False)                # SHA256 哈希存储
    role = Column(String, nullable=False)                    # root / admin / student
    company_id = Column(Integer, ForeignKey("companies.id"), nullable=True)  # root 为 NULL
    created_at = Column(DateTime, server_default=func.now())
