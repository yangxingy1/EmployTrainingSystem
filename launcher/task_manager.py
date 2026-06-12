import json

TASK_FILE = "current_task.json"


def save_task(assignment_id, task_id, status="running"):

    with open(TASK_FILE, "w", encoding="utf-8") as f:

        json.dump(
            {
                "assignment_id": assignment_id,
                "task_id": task_id,
                "status": status
            },
            f,
            indent=4
        )


def load_task():
    with open(
            TASK_FILE,
            "r",
            encoding="utf-8"
    ) as f:
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
