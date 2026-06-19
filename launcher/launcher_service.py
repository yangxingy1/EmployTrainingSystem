import subprocess
import os
import requests
import time

from uploader import upload_result
from config_manager import load_config
from task_manager import save_task, load_task_id

running_process = None


def is_training():
    global running_process

    if running_process is None:
        return False

    return running_process.poll() is None


def _launch_args(context):
    args = []
    mapping = {
        "student_id": "--student-id",
        "assignment_id": "--assignment-id",
        "task_id": "--task-id",
        "attempt_id": "--attempt-id",
        "scene_name": "--scene-name",
        "backend_url": "--backend-url",
    }
    for key, flag in mapping.items():
        value = context.get(key)
        if value is not None and value != "":
            args.extend([flag, str(value)])
    return args


def start_training(exe_path, context=None):
    global running_process
    context = context or {}

    if is_training():
        print("训练已在运行")

        return {
            "success": False,
            "running": True,
            "error_code": "already_running",
            "message": "训练已在运行",
        }

    if not os.path.exists(exe_path):
        print("训练程序不存在")

        return {
            "success": False,
            "running": False,
            "error_code": "exe_not_found",
            "message": "训练程序不存在",
        }

    print("启动训练")

    exe_dir = os.path.dirname(os.path.abspath(exe_path))
    command = [exe_path] + _launch_args(context)
    try:
        running_process = subprocess.Popen(
            command,
            cwd=exe_dir if exe_dir else None
        )
    except Exception as e:
        print("训练启动失败:", e)
        running_process = None
        return {
            "success": False,
            "running": False,
            "error_code": "launch_failed",
            "message": str(e),
        }

    return {
        "success": True,
        "running": True,
        "message": "训练已启动",
    }


def check_finished():
    global running_process

    if running_process is None:
        return False

    if running_process.poll() is not None:
        print("训练结束")

        running_process = None

        config = load_config()
        if config.get("legacy_result_upload"):
            result = upload_result()
            mark_done()

        return True

    return False


def start_training_with_task(assignment_id, task_id):
    print(
        f"启动任务: {assignment_id}, {task_id}"
    )

    save_task(
        assignment_id,
        task_id
    )

    config = load_config()

    start_training(
        config["trainer_exe"],
        {
            "assignment_id": assignment_id,
            "task_id": task_id,
            "backend_url": config.get("server_url"),
        },
    )


def poll_server():
    config = load_config()

    student_id = config.get("student_id")

    if not student_id:
        print("未绑定用户，停止轮询")

        return

    try:

        response = requests.get(
            f"{config['server_url']}/task/launcher/task/{student_id}",
            timeout=5
        )

        data = response.json()

        print("后端返回：", data)

        if data.get("start_training"):
            print("发现待训练任务")

            start_training_with_task(
                data["assignment_id"],
                data["task_id"]
            )

    except Exception as e:

        print("轮询失败:", e)


def mark_done():
    try:
        config = load_config()

        assignment_id = load_task_id()  # 你task_manager里应该有

        if not assignment_id:
            print("没有assignment_id，无法标记完成")
            return

        response = requests.post(
            f"{config['server_url']}/task/finish",
            json={
                "assignment_id": assignment_id
            },
            timeout=5
        )

        print("完成任务已通知后端:", response.json())

    except Exception as e:
        print("mark_done失败:", e)
