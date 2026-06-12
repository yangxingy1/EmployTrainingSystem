import json
import os

# 配置文件路径：始终相对于 launcher 目录
CONFIG_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")


def load_config():
    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def save_config(config):
    print("CONFIG绝对路径:", os.path.abspath(CONFIG_PATH))
    with open(CONFIG_PATH, "w", encoding="utf-8") as f:
        json.dump(config, f, indent=4)


def bind_user(student_id, username, token):
    """绑定学员信息到配置文件"""
    config = load_config()
    config["student_id"] = student_id
    config["username"] = username
    config["token"] = token
    save_config(config)


def get_student_id():
    return load_config().get("student_id")


def get_token():
    return load_config().get("token")
