# 任务分配记录 —— 关联用户与训练任务

from sqlalchemy import (
    Column,
    Integer,
    String,
    ForeignKey,
    DateTime
)

from sqlalchemy.sql import func

from backend.database import Base


class TaskAssignment(Base):

    __tablename__ = "task_assignments"

    id = Column(
        Integer,
        primary_key=True,
        index=True
    )

    user_id = Column(
        Integer,
        ForeignKey("users.id")
    )

    task_id = Column(
        Integer,
        ForeignKey("tasks.id")
    )

    status = Column(
        String,
        default="pending"
    )

    score = Column(
        Integer,
        nullable=True
    )

    train_time = Column(
        Integer,
        nullable=True
    )

    finished_at = Column(
        DateTime,
        nullable=True
    )

    created_at = Column(
        DateTime,
        server_default=func.now()
    )