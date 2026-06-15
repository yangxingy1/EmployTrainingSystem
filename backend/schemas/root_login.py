# Root 登录请求体
from pydantic import BaseModel


class RootLoginRequest(BaseModel):
    username: str
    password: str
