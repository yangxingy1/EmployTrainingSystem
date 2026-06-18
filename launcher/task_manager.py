import json
import os
import sys

if getattr(sys, "frozen", False):
    BASE_DIR = os.path.dirname(sys.executable)
else:
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))

TASK_FILE = os.path.join(BASE_DIR, "current_task.json")


def save_task(assignment_id, task_id, status="running", **extra):
    payload = {"assignment_id": assignment_id, "task_id": task_id, "status": status}
    payload.update({key: value for key, value in extra.items() if value is not None})
    with open(TASK_FILE, "w", encoding="utf-8") as f:
        json.dump(
            payload,
            f,
            indent=4
        )


def load_task():
    with open(TASK_FILE, "r", encoding="utf-8") as f:
        return json.load(f)


def clear_task():
    with open(TASK_FILE, "w", encoding="utf-8") as f:
        json.dump({}, f)


def load_task_id():
    try:
        task = load_task()
        return task.get("assignment_id")
    except:
        return None
