import json
import os
import sys

# 配置文件路径：始终在可执行文件所在目录（适配 exe 打包）
if getattr(sys, "frozen", False):
    BASE_DIR = os.path.dirname(sys.executable)
else:
    BASE_DIR = os.path.dirname(os.path.abspath(__file__))

CONFIG_PATH = os.path.join(BASE_DIR, "config.json")

# server_url 优先从环境变量读取，否则用默认值（部署时修改环境变量或 config.json）
DEFAULT_SERVER_URL = os.environ.get("GTS_SERVER_URL", "http://127.0.0.1:8000")

DEFAULT_CONFIG = {
    "server_url": DEFAULT_SERVER_URL,
    "student_id": 0,
    "username": "",
    "token": "",
    "poll_interval": 5,
    "trainer_exe": "unity/main.exe"
}


def load_config():
    """加载配置，文件不存在时自动创建默认配置"""
    if not os.path.exists(CONFIG_PATH):
        save_config(DEFAULT_CONFIG)
    with open(CONFIG_PATH, "r", encoding="utf-8") as f:
        return json.load(f)


def save_config(config):
    with open(CONFIG_PATH, "w", encoding="utf-8") as f:
        json.dump(config, f, indent=4, ensure_ascii=False)


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
