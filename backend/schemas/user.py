# 注册请求体校验 —— Pydantic 模型
from typing import Optional
from pydantic import BaseModel, Field


class UserCreate(BaseModel):
    """注册请求体（仅限学员，role 固定为 student）"""
    username: str = Field(..., min_length=1, max_length=50)
    password: str = Field(..., min_length=3, max_length=100)
    role: str = Field(..., pattern="^student$")  # 正则限制仅允许 student
    company_id: int
