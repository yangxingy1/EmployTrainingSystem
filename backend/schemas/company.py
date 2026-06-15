# 公司相关 Pydantic 请求体 —— 供 Root 路由使用
from pydantic import BaseModel


class CompanyCreate(BaseModel):
    """创建公司请求体"""
    name: str
    code: str


class AdminCreate(BaseModel):
    """创建管理员请求体"""
    username: str
    password: str
    company_id: int
