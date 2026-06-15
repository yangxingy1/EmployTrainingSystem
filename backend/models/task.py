# 训练任务模型 —— 管理员创建后分配给学员
from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.sql import func
from backend.database import Base


class Task(Base):
    __tablename__ = "tasks"

    id = Column(Integer, primary_key=True)
    title = Column(String, nullable=False)          # 训练名称
    description = Column(String)                     # 训练说明 / 操作指南
    scene_name = Column(String)                      # Unity 场景名称（预留）
    created_at = Column(DateTime, server_default=func.now())
