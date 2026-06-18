from typing import Any, Optional

from pydantic import BaseModel, Field


class TrainingStartRequest(BaseModel):
    student_id: int = Field(..., gt=0)
    assignment_id: int = Field(..., gt=0)


class TrainingCancelRequest(BaseModel):
    student_id: int = Field(..., gt=0)
    reason: Optional[str] = None


class TrainingResultSubmit(BaseModel):
    attempt_id: int = Field(..., gt=0)
    student_id: int = Field(..., gt=0)
    scene_name: str
    sub_task_id: Optional[str] = None
    score: int = Field(..., ge=0, le=100)
    train_time: int = Field(..., ge=0)
    started_at: Optional[str] = None
    finished_at: Optional[str] = None
    steps: list[dict[str, Any]] = Field(default_factory=list)
    errors: list[dict[str, Any]] = Field(default_factory=list)
