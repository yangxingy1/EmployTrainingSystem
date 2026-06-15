# 公司模型 —— 平台下可创建多个公司，每公司独立管理学员
from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.sql import func
from backend.database import Base


class Company(Base):
    __tablename__ = "companies"

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String(100), unique=True, nullable=False)  # 公司全称
    code = Column(String(50), unique=True, nullable=False)   # 公司编码，用于标识
    status = Column(String(20), default="active")             # active / inactive
    created_at = Column(DateTime, server_default=func.now())
