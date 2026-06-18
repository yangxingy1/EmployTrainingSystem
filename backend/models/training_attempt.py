from sqlalchemy import Column, DateTime, ForeignKey, Integer, String, Text
from sqlalchemy.sql import func

from backend.database import Base


class TrainingAttempt(Base):
    __tablename__ = "training_attempts"

    id = Column(Integer, primary_key=True, index=True)
    assignment_id = Column(Integer, ForeignKey("task_assignments.id"), nullable=False)
    student_id = Column(Integer, ForeignKey("users.id"), nullable=False)
    task_id = Column(Integer, ForeignKey("tasks.id"), nullable=False)
    scene_name = Column(String, nullable=False)
    sub_task_id = Column(String, nullable=True)
    status = Column(String, default="running")
    score = Column(Integer, nullable=True)
    train_time = Column(Integer, nullable=True)
    started_at = Column(DateTime, server_default=func.now())
    finished_at = Column(DateTime, nullable=True)
    details_json = Column(Text, nullable=True)
    error_count = Column(Integer, default=0)
    safety_error_count = Column(Integer, default=0)
    summary = Column(Text, nullable=True)
