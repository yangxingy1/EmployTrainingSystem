# Python 手势识别服务

该目录负责摄像头采集、MediaPipe 手部关键点识别、规则式手势识别和 WebSocket 数据广播。Unity 端不直接调用摄像头，而是通过 WebSocket 接收这里发出的数据。

## 安装依赖

```powershell
python -m venv .venv
.\.venv\Scripts\activate
pip install -r requirements.txt
```

也可以在工程根目录直接运行：

```powershell
.\scripts\setup_python.ps1
```

## 摄像头模式

```powershell
python -m src.main --mode camera
```

或在工程根目录运行：

```powershell
.\scripts\run_camera.ps1
```

## 回放模式

没有摄像头、摄像头被占用，或者只想验证 Unity 接收逻辑时，可以使用回放模式：

```powershell
python -m src.main --mode replay --replay samples/replay_sample.jsonl
```

工程根目录快捷命令：

```powershell
.\scripts\run_replay.ps1
```

## 输出端口

| URL | Content |
|---|---|
| `ws://localhost:8765` | 原始 21 点手部关键点 |
| `ws://localhost:8766` | 高层手势事件，如抓取、旋转、点击 |

## 后续扩展方向

| 文件 | 建议扩展内容 |
|---|---|
| `src/hand_tracker.py` | 摄像头参数、左右手判断、关键点平滑 |
| `src/recognizer.py` | 增加拧螺丝、按压、拿取、放置等手势规则 |
| `src/recorder.py` | 保存训练过程中的手势数据 |
| `src/replay.py` | 回放更多典型操作样例，方便课堂演示 |
