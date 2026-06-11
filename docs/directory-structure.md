# 目录结构说明

项目不再保留 PowerShell 启动脚本。Python 服务请在 Conda 环境中手动运行 `gesture-service/main.py`。

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
│  ├─ requirements.txt
│  ├─ README.md
│  ├─ samples/
│  └─ src/
└─ unity/
   └─ My project/
      ├─ Assets/Scenes/
      ├─ Assets/Scripts/
      ├─ Packages/
      └─ ProjectSettings/
```

Python 启动：

```powershell
conda activate huidongshou
cd D:\project_training\gesture-service
python main.py --mode camera --camera 0 --backend dshow --preview
```

Unity 场景脚本：

```text
Assets/Scripts/
├─ ElectricSwitch/
└─ RotaryValve/
```
