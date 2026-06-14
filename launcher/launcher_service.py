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


def start_training(exe_path):
    global running_process

    if is_training():
        print("训练已在运行")

        return False

    if not os.path.exists(exe_path):
        print("训练程序不存在")

        return False

    print("启动训练")

    running_process = subprocess.Popen(
        exe_path
    )

    return True


def check_finished():
    global running_process

    if running_process is None:
        return False

    if running_process.poll() is not None:
        print("训练结束")

        running_process = None

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
        config["trainer_exe"]
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
