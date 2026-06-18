from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import os

from config_manager import bind_user, load_config, BASE_DIR as CONFIG_DIR
from launcher_service import start_training, is_training
from task_manager import load_task, save_task

app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


class StartTrainingRequest(BaseModel):
    student_id: int
    username: str = ""
    assignment_id: int
    task_id: int
    attempt_id: int
    scene_name: str = ""
    backend_url: str = ""


class StartEntryRequest(BaseModel):
    student_id: int = 0
    username: str = ""
    backend_url: str = ""


class BindRequest(BaseModel):
    student_id: int
    username: str
    token: str


def resolve_trainer_exe(config):
    trainer_exe = config.get("trainer_exe", "")
    if not trainer_exe:
        return ""

    normalized = os.path.normpath(trainer_exe)
    if os.path.isabs(normalized):
        return normalized

    candidates = [
        os.path.join(CONFIG_DIR, normalized),
        os.path.abspath(normalized),
        os.path.join(os.path.dirname(CONFIG_DIR), normalized),
    ]
    for candidate in candidates:
        if os.path.exists(candidate):
            return candidate

    return candidates[0]


@app.post("/bind")
def bind(data: BindRequest):
    bind_user(data.student_id, data.username, data.token)
    return {"success": True, "message": "绑定成功"}


@app.get("/status")
def status():
    config = load_config()
    exe_abs = resolve_trainer_exe(config)
    try:
        current_task = load_task()
    except Exception:
        current_task = {}
    return {
        "status": "ready",
        "running": is_training(),
        "exe_exists": os.path.exists(exe_abs),
        "trainer_exe": config.get("trainer_exe", ""),
        "trainer_exe_path": exe_abs,
        "current_task": current_task,
    }


@app.post("/start")
def start(data: StartTrainingRequest):
    config = load_config()
    backend_url = data.backend_url or config.get("server_url")
    save_task(
        data.assignment_id,
        data.task_id,
        student_id=data.student_id,
        username=data.username,
        attempt_id=data.attempt_id,
        scene_name=data.scene_name,
        backend_url=backend_url,
    )
    exe_abs = resolve_trainer_exe(config)
    return start_training(
        exe_abs,
        {
            "student_id": data.student_id,
            "assignment_id": data.assignment_id,
            "task_id": data.task_id,
            "attempt_id": data.attempt_id,
            "scene_name": data.scene_name,
            "backend_url": backend_url,
        },
    )


@app.post("/start-entry")
def start_entry(data: StartEntryRequest):
    config = load_config()
    backend_url = data.backend_url or config.get("server_url")
    exe_abs = resolve_trainer_exe(config)
    return start_training(
        exe_abs,
        {
            "student_id": data.student_id,
            "username": data.username,
            "scene_name": "entry",
            "backend_url": backend_url,
        },
    )


@app.get("/training_status")
def training_status():
    return {"running": is_training()}
