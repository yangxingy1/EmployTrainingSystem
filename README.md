# HuiDongShou MVP 项目结构说明

本项目由两部分组成：

- Python 手势服务：读取摄像头画面，使用 MediaPipe 识别真实手部 21 个关键点，并通过 WebSocket 发送给 Unity。
- Unity 训练端：接收手部关键点，显示虚拟手，并驱动当前的拉杆式电闸训练场景。

当前推荐使用 Conda 虚拟环境运行 Python 服务。使用 Conda 后，`gesture-service/.venv/` 不再需要参与运行；它只是旧版 venv 方案留下的本地环境目录，可以删除，也不会影响项目代码。

## 快速运行

第一次配置 Conda 环境：

```powershell
cd D:\project_training
.\scripts\setup_conda.ps1
```

启动摄像头模式：

```powershell
cd D:\project_training
.\scripts\run_camera.ps1
```

没有摄像头时使用回放模式：

```powershell
cd D:\project_training
.\scripts\run_replay.ps1
```

如果要复用已有 Conda 环境，例如 `hand`：

```powershell
$env:HUIDONGSHOU_CONDA_ENV="hand"
.\scripts\setup_conda.ps1
.\scripts\run_camera.ps1
```

Unity 工程目录：

```text
D:\project_training\unity\My project
```

打开 `Assets/Scenes/SampleScene.unity` 后点击 Play。场景会自动生成虚拟手和拉杆式电闸。

## 项目目录结构

```text
project_training/
├─ README.md
├─ .gitignore
├─ docs/
│  ├─ architecture.md
│  ├─ directory-structure.md
│  └─ websocket-protocol.md
├─ scripts/
│  ├─ setup_conda.ps1
│  ├─ run_camera.ps1
│  ├─ run_replay.ps1
│  ├─ scan_cameras.ps1
│  └─ setup_python.ps1
├─ gesture-service/
│  ├─ main.py
│  ├─ README.md
│  ├─ requirements.txt
│  ├─ samples/
│  │  └─ replay_sample.jsonl
│  ├─ src/
│  │  ├─ __init__.py
│  │  ├─ main.py
│  │  ├─ config.py
│  │  ├─ hand_tracker.py
│  │  ├─ recognizer.py
│  │  ├─ schemas.py
│  │  ├─ websocket_hub.py
│  │  ├─ recorder.py
│  │  └─ replay.py
│  ├─ recordings/
│  ├─ tests/
│  ├─ .venv/
│  └─ __pycache__/
└─ unity/
   └─ My project/
      ├─ Assets/
      │  ├─ Scenes/
      │  │  └─ SampleScene.unity
      │  └─ Scripts/
      │     ├─ Core/
      │     ├─ Demo/
      │     ├─ Interaction/
      │     ├─ Prototype/
      │     └─ Training/
      ├─ Packages/
      ├─ ProjectSettings/
      ├─ UserSettings/
      ├─ Library/
      ├─ Logs/
      ├─ obj/
      ├─ Assembly-CSharp.csproj
      └─ My project.sln
```

## 根目录文件

| 路径 | 作用 |
|---|---|
| `README.md` | 项目总说明和目录结构说明。 |
| `.gitignore` | 忽略 Unity 缓存、Python 虚拟环境、运行记录等本地生成文件。 |

## docs 文档目录

| 路径 | 作用 |
|---|---|
| `docs/architecture.md` | 系统架构说明。 |
| `docs/directory-structure.md` | 目录结构说明。 |
| `docs/websocket-protocol.md` | Python 与 Unity 的 WebSocket 消息协议。 |

## scripts 脚本目录

| 路径 | 作用 |
|---|---|
| `scripts/setup_conda.ps1` | 推荐使用。创建 Conda 环境并安装 Python 依赖。默认环境名为 `huidongshou`。 |
| `scripts/run_camera.ps1` | 使用 Conda 启动摄像头实时识别模式。 |
| `scripts/run_replay.ps1` | 使用 Conda 启动回放模式。 |
| `scripts/scan_cameras.ps1` | 使用 Conda 扫描本机摄像头编号和后端可用性。 |
| `scripts/setup_python.ps1` | 旧版 venv 环境安装脚本，保留用于兼容。当前不推荐使用。 |

## gesture-service Python 服务

| 路径 | 作用 |
|---|---|
| `gesture-service/main.py` | Python 服务推荐入口。使用 `python main.py ...` 启动。 |
| `gesture-service/README.md` | Python 服务详细说明。 |
| `gesture-service/requirements.txt` | Python 依赖列表。 |
| `gesture-service/samples/replay_sample.jsonl` | 示例回放数据。 |
| `gesture-service/recordings/` | 运行时录制数据目录，通常不提交。 |
| `gesture-service/tests/` | 预留测试目录。 |
| `gesture-service/.venv/` | 旧版 venv 本地环境目录。使用 Conda 后可以删除。 |
| `gesture-service/__pycache__/` | Python 缓存目录，可删除。 |

### Python src 源码

| 路径 | 作用 |
|---|---|
| `gesture-service/src/main.py` | 服务主逻辑：解析参数、启动摄像头/回放、广播 WebSocket 数据。 |
| `gesture-service/src/config.py` | 服务配置：端口、帧率、抓取和点击阈值。 |
| `gesture-service/src/hand_tracker.py` | MediaPipe Hands 封装，输出 21 个手部关键点。 |
| `gesture-service/src/recognizer.py` | 规则式手势识别，产生 grab、rotate、click 等高层事件。 |
| `gesture-service/src/schemas.py` | 数据结构和 JSON 消息格式。 |
| `gesture-service/src/websocket_hub.py` | WebSocket 广播服务。 |
| `gesture-service/src/recorder.py` | JSONL 数据记录器。 |
| `gesture-service/src/replay.py` | JSONL 回放读取器。 |
| `gesture-service/src/__init__.py` | Python 包标记文件。 |

## unity/My project Unity 工程

| 路径 | 作用 |
|---|---|
| `unity/My project/Assets/Scenes/SampleScene.unity` | 当前主场景。场景中挂载 `SceneBootstrap`，运行时自动搭建训练内容。 |
| `unity/My project/Assets/Scripts/` | Unity C# 脚本目录。 |
| `unity/My project/Packages/` | Unity 包管理配置。 |
| `unity/My project/ProjectSettings/` | Unity 项目设置。 |
| `unity/My project/UserSettings/` | Unity 用户本地设置，通常不提交。 |
| `unity/My project/Library/` | Unity 自动生成缓存目录，不提交，可由 Unity 重建。 |
| `unity/My project/Logs/` | Unity 日志目录，不提交。 |
| `unity/My project/obj/` | C# 编译中间产物，不提交。 |
| `unity/My project/Assembly-CSharp.csproj` | Unity 生成的 C# 工程文件。 |
| `unity/My project/My project.sln` | Unity 生成的解决方案文件。 |

### Unity C# 脚本目录

| 路径 | 作用 |
|---|---|
| `Assets/Scripts/Core/GestureMessage.cs` | Unity 侧手势事件消息结构。 |
| `Assets/Scripts/Core/GestureReceiver.cs` | 接收 Python `8766` 高层手势事件。 |
| `Assets/Scripts/Demo/SceneBootstrap.cs` | 当前场景自动搭建入口。 |
| `Assets/Scripts/Demo/ElectricSwitchTask.cs` | 自动生成拉杆式电闸模型、指示灯、状态文本和手部光标。 |
| `Assets/Scripts/Demo/HandGestureDriver.cs` | 旧版阀门/按钮手势驱动示例。 |
| `Assets/Scripts/Demo/GestureDebugDisplay.cs` | 手势事件调试显示。 |
| `Assets/Scripts/Interaction/ElectricSwitchInteractable.cs` | 当前电闸交互逻辑：手抓横杆，上下拉动竖杆。 |
| `Assets/Scripts/Interaction/ValveInteractable.cs` | 阀门交互示例。 |
| `Assets/Scripts/Interaction/ButtonInteractable.cs` | 按钮交互示例。 |
| `Assets/Scripts/Interaction/IGestureInteractable.cs` | 手势交互对象接口。 |
| `Assets/Scripts/Prototype/HandInput.cs` | 接收 Python `8765` 原始 21 点数据，并映射到 Unity 交互平面。 |
| `Assets/Scripts/Prototype/HandVisual.cs` | 虚拟手可视化。 |
| `Assets/Scripts/Prototype/GraspController.cs` | 旧版抓取控制器。 |
| `Assets/Scripts/Prototype/Grabbable.cs` | 可抓取物体标记。 |
| `Assets/Scripts/Prototype/FreeMoveTask.cs` | 旧版自由抓取方块练习。 |
| `Assets/Scripts/Prototype/PickPlaceTask.cs` | 旧版抓取放置任务。 |
| `Assets/Scripts/Prototype/SnapZone.cs` | 吸附区域。 |
| `Assets/Scripts/Prototype/DemoBootstrap.cs` | 旧版抓取放置场景搭建入口。 |
| `Assets/Scripts/Training/TrainingTaskManager.cs` | 训练流程管理示例。 |
| `Assets/Scripts/Training/OperationLogger.cs` | 操作日志记录。 |

## 不需要提交或可删除的目录

| 路径 | 说明 |
|---|---|
| `gesture-service/.venv/` | 旧版 venv 环境。使用 Conda 后可删除。 |
| `gesture-service/__pycache__/` | Python 缓存。 |
| `gesture-service/src/__pycache__/` | Python 缓存。 |
| `gesture-service/recordings/*.jsonl` | 运行录制数据。 |
| `unity/My project/Library/` | Unity 缓存。 |
| `unity/My project/Temp/` | Unity 临时文件。 |
| `unity/My project/obj/` | C# 中间产物。 |
| `unity/My project/Logs/` | Unity 日志。 |
| `unity/My project/UserSettings/` | 用户本地设置。 |

如果已经切换到 Conda，并确认不再需要旧 venv，可以手动删除：

```powershell
Remove-Item -Recurse -Force .\gesture-service\.venv
```

删除 `.venv` 不会影响 Conda 环境；Conda 环境存放在 Anaconda 的 `envs` 目录中。

## WebSocket 端口

| 地址 | 内容 | Unity 用途 |
|---|---|---|
| `ws://localhost:8765` | 原始手部 21 点关键点 | 当前虚拟手和电闸交互使用。 |
| `ws://localhost:8766` | 高层手势事件 | 预留给阀门、按钮、训练流程判定。 |

## 当前主流程

```text
摄像头画面
-> Python main.py
-> MediaPipe Hands 识别 21 点
-> WebSocket 8765 推送原始手部数据
-> Unity HandInput 接收并映射到场景
-> SceneBootstrap 自动生成拉杆式电闸
-> ElectricSwitchInteractable 根据手部捏合和移动控制竖杆上下滑动
```
