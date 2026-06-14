# Launcher 主入口
# 启动本地 API (:9000) 接收前端指令，按需启动 Unity 训练 exe
import threading
import time
import uvicorn

from launcher_api import app
from launcher_service import check_finished


def start_api():
    uvicorn.run(app, host="127.0.0.1", port=9000, log_level="warning")


def main():
    print("Launcher 启动，等待训练指令...")

    api_thread = threading.Thread(target=start_api, daemon=True)
    api_thread.start()

    # 主循环：仅监控训练是否结束并上报
    while True:
        check_finished()
        time.sleep(2)


if __name__ == "__main__":
    main()
