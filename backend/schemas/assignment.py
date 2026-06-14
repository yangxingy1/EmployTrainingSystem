# 分配任务请求体
from pydantic import BaseModel, Field


class AssignmentCreate(BaseModel):
    """分配任务请求体 —— user_id 和 task_id 必须 > 0"""
    user_id: int = Field(..., gt=0)
    task_id: int = Field(..., gt=0)
