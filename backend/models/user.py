# 用户模型 —— 管理员 / 学员
from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.sql import func
from backend.database import Base


class User(Base):
    __tablename__ = "users"

    id = Column(Integer, primary_key=True, index=True)
    username = Column(String, unique=True, nullable=False)
    password = Column(String, nullable=False)           # SHA256 哈希存储
    role = Column(String, nullable=False)                # "admin" 或 "student"
    created_at = Column(DateTime, server_default=func.now())
