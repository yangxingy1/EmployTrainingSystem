from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel

from config_manager import bind_user, load_config
from launcher_service import start_training, is_training
from task_manager import save_task

app = FastAPI()

# CORS：允许前端页面跨域调用 launcher API
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class StartTrainingRequest(BaseModel):
    assignment_id: int
    task_id: int


class BindRequest(BaseModel):
    student_id: int
    username: str
    token: str


@app.post("/bind")
def bind(data: BindRequest):
    """绑定学员身份到 launcher"""
    bind_user(data.student_id, data.username, data.token)
    return {"success": True, "message": "绑定成功"}


@app.get("/status")
def status():
    return {"status": "ready"}


@app.post("/start")
def start(data: StartTrainingRequest):
    """启动训练 exe"""
    config = load_config()
    save_task(data.assignment_id, data.task_id)
    success = start_training(config["trainer_exe"])
    return {"success": success}


@app.get("/training_status")
def training_status():
    return {"running": is_training()}
