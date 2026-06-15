# 登录请求体
from typing import Optional
from pydantic import BaseModel


class LoginRequest(BaseModel):
    username: str
    password: str
    company_id: Optional[int] = None  # root 登录时不需要，admin/student 必填
