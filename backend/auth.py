# JWT 认证模块 —— 生成和校验 Token
from datetime import datetime, timedelta, timezone
from jose import jwt, JWTError

SECRET_KEY = "gesture_training_secret"
ALGORITHM = "HS256"
ACCESS_TOKEN_EXPIRE_HOURS = 24


def create_access_token(data: dict):
    """根据用户数据生成 JWT Token，默认 24 小时过期"""
    to_encode = data.copy()
    expire = datetime.now(timezone.utc) + timedelta(hours=ACCESS_TOKEN_EXPIRE_HOURS)
    to_encode.update({"exp": expire})
    return jwt.encode(to_encode, SECRET_KEY, algorithm=ALGORITHM)


def verify_token(token: str):
    """校验 Token 有效性，成功返回 payload，失败返回 None"""
    try:
        return jwt.decode(token, SECRET_KEY, algorithms=[ALGORITHM])
    except JWTError:
        return None
