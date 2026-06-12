# 慧动手 MVP 工程启动说明

本目录 `HuiDongShou-MVP` 是当前项目代码的正式入口。项目采用 **Python 手势识别服务 + Unity 虚拟训练端** 的方式运行：

- Python 负责打开摄像头、调用 MediaPipe 识别真实手部 21 个关键点，并通过 WebSocket 发给 Unity。
- Unity 负责显示虚拟手、判断捏合抓取、控制方块移动、松手自由落体，以及后续训练任务和评分。

## 1. 目录说明

```text
HuiDongShou-MVP/
├─ gesture-service/          Python + MediaPipe 手势识别服务
│  ├─ src/                   Python 源码
│  ├─ samples/               回放测试数据
│  └─ requirements.txt       Python 依赖
├─ unity/
│  └─ My project/            当前 Unity Hub 打开的正式 Unity 工程
├─ unity-client/             早期 Unity 脚本骨架备份
├─ scripts/                  常用启动脚本
├─ docs/                     架构、协议、目录说明
└─ README.md                 本说明文件
```

当前主要代码位置：

| 模块 | 路径 | 作用 |
|---|---|---|
| Python 手势识别 | `gesture-service/src/` | 摄像头采集、MediaPipe 识别、WebSocket 推送 |
| Unity 正式工程 | `unity/My project/` | 当前实际运行的 Unity 项目 |
| Unity 手部输入 | `unity/My project/Assets/Scripts/Prototype/HandInput.cs` | 接收 21 点手部数据并计算捏合强度 |
| Unity 虚拟手显示 | `unity/My project/Assets/Scripts/Prototype/HandVisual.cs` | 显示 21 点虚拟手 |
| Unity 抓取逻辑 | `unity/My project/Assets/Scripts/Prototype/GraspController.cs` | 判断靠近、抓取、移动、释放 |
| Unity 当前场景入口 | `unity/My project/Assets/Scripts/Demo/SceneBootstrap.cs` | 自动搭建训练场景 |
| Unity 自由抓取任务 | `unity/My project/Assets/Scripts/Prototype/FreeMoveTask.cs` | 生成方块、边界、状态文字 |

## 2. 首次环境配置

请使用 Windows PowerShell。

进入项目根目录：

```powershell
cd E:\Desktop\慧动手\HuiDongShou-MVP（有具体根据自己的路径修改）
```

安装 Python 依赖：

```powershell
.\scripts\setup_python.ps1
```

脚本会自动创建虚拟环境并安装依赖。默认虚拟环境位置是：

```text
E:\HuiDongShouPython\.venv
```

这样做是为了避免中文路径导致部分 MediaPipe 原生资源加载异常。

如果你想指定虚拟环境位置，可以先设置环境变量：

```powershell
$env:HUIDONGSHOU_VENV="E:\HuiDongShouPython\.venv"
.\scripts\setup_python.ps1
```

## 3. 启动 Python 手势服务

摄像头实时模式：

```powershell
cd E:\Desktop\慧动手\HuiDongShou-MVP
.\scripts\run_camera.ps1
```

正常启动后，终端会看到类似：

```text
[raw-hand] listening on ws://localhost:8765
[gesture-event] listening on ws://localhost:8766
[main] camera mode started. camera=0, backend=dshow. Press Ctrl+C to stop.
```

同时会弹出预览窗口 `HuiDongShou Gesture Preview`。如果窗口里能看到手部骨架，说明 Python 端识别正常。

如果没有摄像头，或者只想测试 Unity 连接，可以运行回放模式：

```powershell
cd E:\Desktop\慧动手\HuiDongShou-MVP
.\scripts\run_replay.ps1
```

## 4. 启动 Unity 端

1. 打开 Unity Hub。
2. 选择“添加/打开项目”。
3. 打开以下目录：

```text
E:\Desktop\慧动手\HuiDongShou-MVP\unity\My project
```

4. 等待 Unity 编译完成。
5. 确认 Python 手势服务已经启动。
6. 在 Unity 顶部点击 Play。

当前 Unity 会自动生成自由抓取训练场景，标题为：

```text
Free grab practice: release to drop
```

画面中应出现虚拟手、三个方块、边界和左上角调试文字。

## 5. 当前交互逻辑

当前版本主要实现“真实手势抓取方块”的基础原型：

```text
摄像头画面
  -> Python + MediaPipe 识别 21 个手部关键点
  -> ws://127.0.0.1:8765 推送给 Unity
  -> Unity 显示虚拟手
  -> Unity 判断拇指和食指捏合
  -> 靠近方块并持续捏合后抓取
  -> 手移动时方块跟随
  -> 松手后方块自由落体
```

当前关键阈值：

| 参数 | 当前值 | 含义 |
|---|---:|---|
| 抓取阈值 | `0.58` | 捏合强度超过该值才可能抓取 |
| 释放阈值 | `0.30` | 捏合强度低于该值后释放 |
| 抓取确认时间 | `0.08s` | 避免扫过方块时误抓 |
| 释放确认时间 | `0.05s` | 松手后快速释放 |

## 6. WebSocket 端口

| 端口 | 数据内容 | 当前用途 |
|---|---|---|
| `8765` | 原始 21 点手部关键点 | 当前 Unity 主流程使用 |
| `8766` | 高层手势事件 | 预留给阀门、按钮、复杂任务 |

当前 Unity 抓取主要依赖 `8765`。也就是说，Python 只负责识别手部关键点，抓取判断主要在 Unity 中执行。

## 7. 常见问题

### 7.1 Unity 提示连接失败

先确认 Python 服务是否启动：

```powershell
cd E:\Desktop\慧动手\HuiDongShou-MVP
.\scripts\run_camera.ps1
```

Unity 需要连接：

```text
ws://127.0.0.1:8765
```

如果 Python 终端没有显示 `listening on ws://localhost:8765`，Unity 就连不上。

### 7.2 Python 预览窗口黑屏

可能原因：

- 摄像头被微信、浏览器、会议软件占用。
- Windows 摄像头权限没有打开。
- 摄像头编号不是 `0`。
- 光线太暗。

可以先关闭其他占用摄像头的软件，再重新运行：

```powershell
.\scripts\run_camera.ps1
```

### 7.3 Python 能识别手，但 Unity 没有虚拟手

检查顺序：

1. Unity 是否打开的是 `unity\My project`。
2. Python 服务是否还在运行。
3. Unity Console 是否有红色报错。
4. 是否在 Unity 中点击了 Play。

### 7.4 没有抓取成功

看 Unity 左上角调试文字：

```text
抓取判定: 当前值 / 0.58
捏合强度
握拳强度 (不参与)
```

只有抓取判定超过 `0.58`，并且手部光标靠近方块，才会抓住。

### 7.5 没抓住但物体移动

当前自由抓取模式已经关闭手部物理碰撞，理论上不会再被虚拟手直接推走。如果仍然出现该问题，优先检查 Unity Console 是否有旧脚本残留或红色报错。

## 8. 推荐开发顺序

当前已经完成基础“真实手势抓取”原型。后续建议按以下顺序开发：

1. 手势校准：根据不同用户手型自动设置捏合阈值。
2. 抓取放置任务：将不同颜色工件移动到指定区域。
3. 评分系统：统计用时、掉落次数、误抓次数、放置精度。
4. 训练记录：保存每次训练结果为 JSON 或 CSV。
5. 工厂场景优化：替换方块为零件、阀门、按钮、工具等。
6. 多任务训练流程：开始训练、任务提示、完成判定、结果页。
