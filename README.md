# HuiDongShou MVP 项目说明

本项目由两个端组成：

- Python 手势识别服务：使用 OpenCV + MediaPipe 读取摄像头并识别手部 21 个关键点，通过 WebSocket 发送给 Unity。
- Unity 工业实训端：接收手部数据，显示虚拟手，并运行工业训练场景，例如拉杆式电闸和旋转阀门。

现在不再使用 PowerShell 启动脚本。请先手动进入 Conda 环境，再用 `python main.py` 启动 Python 服务。

## Python 运行方式

创建 Conda 环境：

```powershell
conda create -n huidongshou python=3.11 pip
conda activate huidongshou
cd D:\project_training\gesture-service
pip install -r requirements.txt
```

摄像头实时模式：

```powershell
conda activate huidongshou
cd D:\project_training\gesture-service
python main.py --mode camera --camera 0 --backend dshow --preview
```

回放模式：

```powershell
conda activate huidongshou
cd D:\project_training\gesture-service
python main.py --mode replay --replay samples\replay_sample.jsonl
```

扫描摄像头可以直接在已激活的 Conda 环境中执行：

```powershell
python -c "import cv2; print(cv2.VideoCapture(0, cv2.CAP_DSHOW).isOpened())"
```

如果已经统一使用 Conda，`gesture-service/.venv/` 不再需要，可以删除。

## Unity 运行方式

用 Unity Hub 打开：

```text
D:\project_training\unity\My project
```

打开场景：

```text
Assets/Scenes/SampleScene.unity
```

点击 Play。`SceneBootstrap` 会自动创建虚拟手和当前训练场景。当前默认场景是 `RotaryValve`，可以在 Inspector 里把 `sceneKind` 切换为：

- `RotaryValve`：旋转型阀门训练。
- `ElectricSwitch`：拉杆式电闸训练。

## 项目目录结构

```text
project_training/
├─ README.md
├─ .gitignore
├─ docs/
│  ├─ architecture.md
│  ├─ directory-structure.md
│  └─ websocket-protocol.md
├─ gesture-service/
│  ├─ main.py
│  ├─ README.md
│  ├─ requirements.txt
│  ├─ samples/
│  │  └─ replay_sample.jsonl
│  └─ src/
│     ├─ __init__.py
│     ├─ main.py
│     ├─ config.py
│     ├─ hand_tracker.py
│     ├─ recognizer.py
│     ├─ schemas.py
│     ├─ websocket_hub.py
│     ├─ recorder.py
│     └─ replay.py
└─ unity/
   └─ My project/
      ├─ Assets/
      │  ├─ Scenes/
      │  │  ├─ ElectricSwitch/
      │  │  ├─ RotaryValve/
      │  │  └─ SampleScene.unity
      │  └─ Scripts/
      │     ├─ Core/
      │     ├─ Demo/
      │     ├─ ElectricSwitch/
      │     ├─ Interaction/
      │     ├─ Prototype/
      │     ├─ RotaryValve/
      │     └─ Training/
      ├─ Packages/
      └─ ProjectSettings/
```

## Python 文件作用

| 路径 | 作用 |
|---|---|
| `gesture-service/main.py` | 推荐入口。运行 `python main.py ...`。 |
| `gesture-service/requirements.txt` | Python 依赖列表。 |
| `gesture-service/samples/replay_sample.jsonl` | 回放示例数据。 |
| `gesture-service/src/main.py` | 服务主逻辑：解析参数、启动摄像头/回放、启动 WebSocket。 |
| `gesture-service/src/config.py` | 服务配置：端口、帧率、抓取/点击阈值。 |
| `gesture-service/src/hand_tracker.py` | MediaPipe Hands 封装，输出 21 个手部关键点。 |
| `gesture-service/src/recognizer.py` | 规则式手势识别，生成 grab、rotate、click 等事件。 |
| `gesture-service/src/schemas.py` | 数据结构和 JSON 消息格式。 |
| `gesture-service/src/websocket_hub.py` | WebSocket 广播服务。 |
| `gesture-service/src/recorder.py` | JSONL 数据记录器。 |
| `gesture-service/src/replay.py` | JSONL 回放读取器。 |

## Unity 脚本结构

| 路径 | 作用 |
|---|---|
| `Assets/Scripts/Core/` | WebSocket 接收和消息结构。 |
| `Assets/Scripts/Demo/SceneBootstrap.cs` | 当前场景自动搭建入口，可切换训练场景。 |
| `Assets/Scripts/Interaction/` | 通用交互接口和旧示例。 |
| `Assets/Scripts/ElectricSwitch/` | 拉杆式电闸场景专属脚本。 |
| `Assets/Scripts/RotaryValve/` | 旋转型阀门场景专属脚本。 |
| `Assets/Scripts/Prototype/` | 手部输入、虚拟手显示、旧抓取原型。 |
| `Assets/Scripts/Training/` | 训练流程、日志和评分示例。 |

## WebSocket 端口

| 地址 | 内容 | Unity 用途 |
|---|---|---|
| `ws://localhost:8765` | 原始手部 21 点关键点 | 当前虚拟手和场景交互使用。 |
| `ws://localhost:8766` | 高层手势事件 | 预留给阀门、按钮、训练流程判定。 |

## 不需要提交的内容

这些目录由 `.gitignore` 忽略：

- `gesture-service/.venv/`
- `gesture-service/__pycache__/`
- `gesture-service/recordings/`
- `unity/My project/Library/`
- `unity/My project/Temp/`
- `unity/My project/obj/`
- `unity/My project/Logs/`
- `unity/My project/UserSettings/`
