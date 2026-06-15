# 训练任务模型
from sqlalchemy import Column, Integer, String, DateTime
from sqlalchemy.sql import func
from backend.database import Base


class Task(Base):

    __tablename__ = "tasks"

    id = Column(Integer, primary_key=True)

    title = Column(String, nullable=False)

    description = Column(String)

    scene_name = Column(String)

    created_at = Column(
        DateTime,
        server_default=func.now()
    )