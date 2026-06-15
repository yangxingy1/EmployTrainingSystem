# 数据库配置: SQLite + SQLAlchemy
from pathlib import Path
from sqlalchemy import create_engine
from sqlalchemy.orm import declarative_base, sessionmaker

BASE_DIR = Path(__file__).resolve().parent
DATA_DIR = BASE_DIR / 'data'
DATA_DIR.mkdir(exist_ok=True)
DATABASE_PATH = DATA_DIR / "gesture.db"
DATABASE_URL = f"sqlite:///{DATABASE_PATH}"

# SQLite 需要 check_same_thread=False 以支持多线程
engine = create_engine(DATABASE_URL, connect_args={"check_same_thread": False})

SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

# 所有 ORM 模型继承此类
Base = declarative_base()


def get_db():
    """FastAPI 依赖注入: 每个请求获取一个会话, 结束后自动关闭"""
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()
