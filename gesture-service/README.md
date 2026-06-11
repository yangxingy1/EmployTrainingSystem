# Python 手势识别服务

本目录负责摄像头采集、MediaPipe 手部关键点识别、手势事件识别和 WebSocket 数据发送。

现在不再使用项目脚本启动。请先手动激活 Conda 环境，再运行 `main.py`。

## 安装依赖

```powershell
conda create -n huidongshou python=3.11 pip
conda activate huidongshou
cd D:\project_training\gesture-service
pip install -r requirements.txt
```

## 摄像头模式

```powershell
conda activate huidongshou
cd D:\project_training\gesture-service
python main.py --mode camera --camera 0 --backend dshow --preview
```

## 回放模式

```powershell
conda activate huidongshou
cd D:\project_training\gesture-service
python main.py --mode replay --replay samples\replay_sample.jsonl
```

## 文件作用

| 文件 | 作用 |
|---|---|
| `main.py` | 推荐入口，调用 `src.main.main()`。 |
| `requirements.txt` | Python 依赖列表。 |
| `src/main.py` | 服务主逻辑：摄像头/回放、WebSocket、识别调度。 |
| `src/config.py` | 端口、帧率和识别阈值配置。 |
| `src/hand_tracker.py` | MediaPipe Hands 封装。 |
| `src/recognizer.py` | 规则式手势识别。 |
| `src/schemas.py` | JSON 消息结构。 |
| `src/websocket_hub.py` | WebSocket 广播。 |
| `src/recorder.py` | JSONL 数据记录。 |
| `src/replay.py` | JSONL 数据回放。 |
| `samples/replay_sample.jsonl` | 回放示例。 |

## 输出端口

| 地址 | 内容 |
|---|---|
| `ws://localhost:8765` | 原始 21 点手部关键点。 |
| `ws://localhost:8766` | 高层手势事件，如 grab、rotate、click。 |
