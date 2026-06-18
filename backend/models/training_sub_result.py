from sqlalchemy import Column, DateTime, ForeignKey, Integer, String, Text, UniqueConstraint
from sqlalchemy.sql import func

from backend.database import Base


class TrainingSubResult(Base):
    __tablename__ = "training_sub_results"
    __table_args__ = (
        UniqueConstraint("attempt_id", "sub_task_id", name="uq_training_sub_result_attempt_sub_task"),
    )

    id = Column(Integer, primary_key=True, index=True)
    attempt_id = Column(Integer, ForeignKey("training_attempts.id"), nullable=False)
    student_id = Column(Integer, ForeignKey("users.id"), nullable=False)
    task_id = Column(Integer, ForeignKey("tasks.id"), nullable=False)
    scene_name = Column(String, nullable=False)
    sub_task_id = Column(String, nullable=False)
    sub_task_name = Column(String, nullable=False)
    status = Column(String, default="done")
    score = Column(Integer, nullable=True)
    train_time = Column(Integer, nullable=True)
    started_at = Column(String, nullable=True)
    finished_at = Column(String, nullable=True)
    steps_json = Column(Text, nullable=True)
    errors_json = Column(Text, nullable=True)
    details_json = Column(Text, nullable=True)
    error_count = Column(Integer, default=0)
    safety_error_count = Column(Integer, default=0)
    summary = Column(Text, nullable=True)
    created_at = Column(DateTime, server_default=func.now())
    updated_at = Column(DateTime, server_default=func.now(), onupdate=func.now())
