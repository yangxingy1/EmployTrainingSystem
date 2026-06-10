# 目录结构说明

```text
project_training/
├─ gesture-service/
│  ├─ main.py                  Python 服务推荐入口
│  ├─ requirements.txt         Python 依赖
│  ├─ README.md                Python 服务说明
│  ├─ samples/                 回放样例
│  └─ src/                     Python 源码包
│     ├─ main.py               服务主逻辑
│     ├─ config.py             服务配置
│     ├─ hand_tracker.py       MediaPipe 手部追踪
│     ├─ recognizer.py         规则式手势识别
│     ├─ schemas.py            数据结构和 JSON 消息格式
│     ├─ websocket_hub.py      WebSocket 广播
│     ├─ recorder.py           JSONL 记录器
│     ├─ replay.py             JSONL 回放读取器
│     └─ __init__.py           包标记
├─ unity/
│  └─ My project/              当前 Unity 工程
│     ├─ Assets/Scenes/        Unity 场景
│     └─ Assets/Scripts/       Unity C# 脚本
│        ├─ Core/              WebSocket 接收和消息结构
│        ├─ Demo/              自动搭建演示场景
│        ├─ Interaction/       可交互设备对象
│        ├─ Prototype/         手部输入、虚拟手、旧抓取原型
│        └─ Training/          训练流程、日志、评分
├─ docs/                       架构和协议文档
└─ scripts/
   ├─ setup_conda.ps1          推荐：创建/安装 Conda 环境
   ├─ run_camera.ps1           Conda 摄像头模式启动脚本
   ├─ run_replay.ps1           Conda 回放模式启动脚本
   ├─ scan_cameras.ps1         Conda 摄像头扫描脚本
   └─ setup_python.ps1         旧版 venv 安装脚本，保留用于兼容
```

## 推荐运行入口

现在 Python 服务推荐使用 Conda：

```powershell
.\scripts\setup_conda.ps1
.\scripts\run_camera.ps1
```

脚本默认使用 Conda 环境 `huidongshou`。如需换名：

```powershell
$env:HUIDONGSHOU_CONDA_ENV="my-hand-env"
.\scripts\setup_conda.ps1
.\scripts\run_camera.ps1
```

Python 服务本体从 `gesture-service/main.py` 启动：

```powershell
cd gesture-service
conda run -n huidongshou python main.py --mode camera --preview
```
