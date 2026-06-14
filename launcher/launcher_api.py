from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import os

from config_manager import bind_user, load_config, BASE_DIR as CONFIG_DIR
from launcher_service import start_training, is_training
from task_manager import save_task

app = FastAPI()

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
    bind_user(data.student_id, data.username, data.token)
    return {"success": True, "message": "绑定成功"}


@app.get("/status")
def status():
    return {"status": "ready"}


@app.post("/start")
def start(data: StartTrainingRequest):
    config = load_config()
    save_task(data.assignment_id, data.task_id)
    exe_abs = os.path.join(CONFIG_DIR, config["trainer_exe"])
    success = start_training(exe_abs)
    return {"success": success}


@app.get("/training_status")
def training_status():
    return {"running": is_training()}
