# 任务分配记录 —— 关联学员与训练任务，追踪状态和成绩
from sqlalchemy import Column, Integer, String, ForeignKey, DateTime
from sqlalchemy.sql import func
from backend.database import Base


class TaskAssignment(Base):
    __tablename__ = "task_assignments"

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(Integer, ForeignKey("users.id"))       # 学员 ID
    task_id = Column(Integer, ForeignKey("tasks.id"))       # 训练项目 ID
    status = Column(String, default="pending")               # pending / running / done
    score = Column(Integer, nullable=True)                   # 训练成绩（完成时写入）
    train_time = Column(Integer, nullable=True)              # 训练用时 秒（完成时写入）
    finished_at = Column(DateTime, nullable=True)            # 完成时间
    created_at = Column(DateTime, server_default=func.now())
