# Python 手势服务说明

这个目录负责摄像头采集、MediaPipe 手部 21 点识别、规则式手势事件识别，以及通过 WebSocket 把数据发送给 Unity。

推荐入口已经改为本目录下的 `main.py`：

```powershell
conda run -n huidongshou python main.py --mode camera --preview
```

旧的 `python -m src.main` 仍然可用，但不再作为推荐运行方式。

## Conda 环境

默认 Conda 环境名是 `huidongshou`。在项目根目录运行：

```powershell
.\scripts\setup_conda.ps1
```

如果想换环境名：

```powershell
$env:HUIDONGSHOU_CONDA_ENV="my-hand-env"
.\scripts\setup_conda.ps1
```

如果想指定 Python 版本：

```powershell
$env:HUIDONGSHOU_PYTHON_VERSION="3.11"
.\scripts\setup_conda.ps1
```

## 启动方式

摄像头实时模式：

```powershell
cd D:\project_training
.\scripts\run_camera.ps1
```

等价的手动命令：

```powershell
cd D:\project_training\gesture-service
conda run -n huidongshou python main.py --mode camera --camera 0 --backend dshow --preview
```

回放模式：

```powershell
cd D:\project_training
.\scripts\run_replay.ps1
```

等价的手动命令：

```powershell
cd D:\project_training\gesture-service
conda run -n huidongshou python main.py --mode replay --replay samples\replay_sample.jsonl
```

扫描摄像头：

```powershell
cd D:\project_training
.\scripts\scan_cameras.ps1
```

## 文件作用

| 文件 | 作用 |
|---|---|
| `main.py` | 推荐启动入口。它调用 `src.main.main()`，让运行命令变成 `python main.py ...`。 |
| `requirements.txt` | Python 依赖列表，包括 `opencv-contrib-python`、`mediapipe`、`websockets` 等。 |
| `src/main.py` | 服务主逻辑：解析命令行、启动摄像头/回放、启动 WebSocket、调用识别和广播。 |
| `src/config.py` | 服务配置：端口、帧率、抓取/点击阈值。 |
| `src/hand_tracker.py` | MediaPipe Hands 封装：输入 OpenCV BGR 图像，输出 21 个归一化手部关键点。 |
| `src/recognizer.py` | 规则式手势识别：根据关键点计算捏合、握拳、抓取、旋转、点击事件。 |
| `src/schemas.py` | 数据结构和消息格式：`Landmark`、`GestureEvent`、原始手部消息 JSON。 |
| `src/websocket_hub.py` | WebSocket 广播中心：维护客户端连接并向 Unity 推送 JSON。 |
| `src/recorder.py` | JSONL 记录器：可把原始手部数据和事件写入文件，方便调试/复现。 |
| `src/replay.py` | JSONL 回放读取器：读取录制数据并按帧发送。 |
| `src/__init__.py` | Python 包标记文件。 |
| `samples/replay_sample.jsonl` | 示例回放数据，没有摄像头时可用于测试 Unity 接收逻辑。 |

## WebSocket 输出

| 地址 | 内容 | Unity 侧用途 |
|---|---|---|
| `ws://localhost:8765` | 原始 21 点手部关键点 | `HandInput.cs` 显示虚拟手，并驱动当前电闸交互。 |
| `ws://localhost:8766` | 高层手势事件，如 grab/rotate/click | 预留给按钮、阀门、训练流程判定等逻辑。 |

原始手部数据格式：

```json
{
  "present": true,
  "x": [0.1],
  "y": [0.2],
  "z": [0.0]
}
```

手势事件格式：

```json
{
  "type": "gesture",
  "seq": 1,
  "timestamp": 1710000000.0,
  "hand": "right",
  "gesture": "grab",
  "state": "start",
  "confidence": 0.9,
  "params": {
    "x": 0.52,
    "y": 0.43,
    "pinchStrength": 0.8
  }
}
```

## 常用参数

```powershell
conda run -n huidongshou python main.py --mode camera --camera 1 --backend msmf --preview
```

| 参数 | 说明 |
|---|---|
| `--mode camera` | 使用真实摄像头。 |
| `--mode replay` | 使用 JSONL 数据回放。 |
| `--camera 0` | 摄像头编号。 |
| `--backend dshow` | Windows OpenCV 摄像头后端，可选 `dshow`、`msmf`、`auto`。 |
| `--preview` | 显示 OpenCV 预览窗口。 |
| `--no-mirror` | 不镜像摄像头画面。 |
| `--record output.jsonl` | 记录运行数据。 |
| `--replay samples\replay_sample.jsonl` | 指定回放文件。 |
