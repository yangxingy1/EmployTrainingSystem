# 人员注册请求体 —— 管理员在本公司内创建管理员或学员
from pydantic import BaseModel, Field


class StaffCreate(BaseModel):
    """管理员创建本公司人员请求体"""
    username: str = Field(..., min_length=1, max_length=50)
    password: str = Field(..., min_length=3, max_length=100)
    role: str = Field(..., pattern="^(admin|student)$")  # 允许 admin 或 student