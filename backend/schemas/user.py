# 请求体校验 —— Pydantic 模型
from pydantic import BaseModel, Field


class UserCreate(BaseModel):
    """注册 / 登录请求体"""
    username: str = Field(..., min_length=1, max_length=50)
    password: str = Field(..., min_length=3, max_length=100)
    role: str = Field(..., pattern="^(student|admin)$")
