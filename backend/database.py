# 数据库配置 —— SQLite + SQLAlchemy
from pathlib import Path
from sqlalchemy import create_engine
from sqlalchemy.orm import declarative_base, sessionmaker

BASE_DIR = Path(__file__).resolve().parent
DATABASE_PATH = BASE_DIR / "gesture.db"
DATABASE_URL = f"sqlite:///{DATABASE_PATH}"

# SQLite 多线程支持
engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False})

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# 所有模型继承 Base
Base = declarative_base()


def get_db():
    """FastAPI 依赖注入：每次请求获取一个数据库会话，结束后自动关闭"""
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
