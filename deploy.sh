#!/bin/bash
# ============================================================
# 慧动手 GestureTrainingSystem — 服务器部署启动脚本
# 用法: bash deploy.sh [start|stop|restart|status]
# ============================================================
set -e

PROJECT_DIR="$(cd "$(dirname "$0")" && pwd)"
BACKEND_PORT=8000
PID_FILE="$PROJECT_DIR/backend.pid"
LOG_FILE="$PROJECT_DIR/backend.log"

start_backend() {
    if [ -f "$PID_FILE" ] && kill -0 $(cat "$PID_FILE") 2>/dev/null; then
        echo "[INFO] 后端已在运行 (PID: $(cat $PID_FILE))"
    else
        echo "[INFO] 启动后端服务 (端口 $BACKEND_PORT)..."
        cd "$PROJECT_DIR"
        nohup python -m uvicorn backend.main:app --host 0.0.0.0 --port $BACKEND_PORT > "$LOG_FILE" 2>&1 &
        echo $! > "$PID_FILE"
        sleep 2
        if kill -0 $(cat "$PID_FILE") 2>/dev/null; then
            echo "[OK] 后端已启动 (PID: $(cat $PID_FILE))"
        else
            echo "[ERROR] 后端启动失败，查看日志: $LOG_FILE"
            exit 1
        fi
    fi
}

stop_backend() {
    if [ -f "$PID_FILE" ]; then
        PID=$(cat "$PID_FILE")
        if kill -0 $PID 2>/dev/null; then
            echo "[INFO] 停止后端 (PID: $PID)..."
            kill $PID
            sleep 1
        fi
        rm -f "$PID_FILE"
        echo "[OK] 后端已停止"
    else
        echo "[INFO] 后端未运行"
    fi
}

init_root() {
    echo "[INFO] 初始化 Root 账号..."
    cd "$PROJECT_DIR"
    python -m backend.init__root
}

case "${1:-start}" in
    start)
        init_root
        start_backend
        echo "[OK] 后端已部署，通过 Nginx 域名访问"
        ;;
    stop)
        stop_backend
        ;;
    restart)
        stop_backend
        sleep 1
        start_backend
        ;;
    status)
        if [ -f "$PID_FILE" ] && kill -0 $(cat "$PID_FILE") 2>/dev/null; then
            echo "[INFO] 后端运行中 (PID: $(cat $PID_FILE))"
        else
            echo "[INFO] 后端未运行"
        fi
        ;;
    *)
        echo "用法: bash deploy.sh [start|stop|restart|status]"
        ;;
esac