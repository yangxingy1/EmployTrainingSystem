# JWT 认证模块 —— Token 生成与校验
import os
import hashlib

from dotenv import load_dotenv
from datetime import datetime, timedelta, timezone
from jose import jwt, JWTError

load_dotenv(os.path.join(os.path.dirname(os.path.abspath(__file__)), ".env"))


SECRET_KEY = os.getenv("SECRET_KEY")
if not SECRET_KEY:
    raise ValueError("SECRET_KEY 未在 .env 中设置")

ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_HOURS = 24


def create_access_token(data: dict):
    """
    生成 JWT Token
    data 必须包含 user_id, username, role, company_id
    默认 24 小时过期
    """
    to_encode = data.copy()
    expire = datetime.now(timezone.utc) + timedelta(hours=ACCESS_TOKEN_EXPIRE_HOURS)
    to_encode.update({"exp": expire})
    return jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)


def verify_token(token: str):
    """
    校验 Token 有效性
    成功返回 payload 字典，失败返回 None
    """
    try:
        return jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
    except JWTError:
        return None


def hash_password(password: str) -> str:
    """SHA256 哈希加密 —— 所有密码入库前均由此函数处理"""
    return hashlib.sha256(password.encode()).hexdigest()