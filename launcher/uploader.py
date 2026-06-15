import json
import os
import requests

from config_manager import load_config
from task_manager import load_task


RESULT_FILE = "result/result.json"


def upload_result():

    if not os.path.exists(
            RESULT_FILE
    ):

        return False

    with open(
            RESULT_FILE,
            "r",
            encoding="utf-8"
    ) as f:

        result = json.load(f)

    config = load_config()

    task = load_task()

    upload_data = {

        "student_id":
            config["student_id"],

        "assignment_id":
            task["assignment_id"],

        "task_id":
            task["task_id"],

        "score":
            result["score"],

        "train_time":
            result["train_time"]
    }

    try:

        response = requests.post(
            f"{config['server_url']}/submit_result",
            json=upload_data,
            timeout=10
        )

        if response.status_code == 200:

            print("成绩上传成功")

            os.remove(
                RESULT_FILE
            )

            return True

        print(
            "上传失败:",
            response.text
        )

        return False

    except Exception as e:

        print(
            "上传异常:",
            e
        )

        return False