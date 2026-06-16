# 慧动手 MVP 工程启动说明

本目录 `HuiDongShou-MVP` 是当前项目代码的正式入口。项目采用 **Python 手势识别服务 + Unity 虚拟训练端** 的方式运行：

- Python 负责打开摄像头、调用 MediaPipe 识别真实手部 21 个关键点，并通过 WebSocket 发给 Unity。
- Unity 负责显示虚拟手、判断捏合抓取、驱动传送带分拣场景、处理投放计数、按钮确认、阀门调节、模块切换和训练评分。

## 1. 目录说明

```text
HuiDongShou-MVP/
├─ gesture-service/          Python + MediaPipe 手势识别服务
│  ├─ src/                   Python 源码
│  ├─ samples/               回放测试数据
│  └─ requirements.txt       Python 依赖
├─ unity/
│  └─ My project/            当前 Unity Hub 打开的正式 Unity 工程
│     └─ Assets/Scripts/
│        ├─ Common/          Unity 手部输入、抓取、吸附等通用能力
│        ├─ Scenes/          各个基础训练场景脚本
│        ├─ Interaction/     按钮、开关等可交互组件
│        ├─ Training/        菜单、导航、训练流程
│        └─ Demo/            运行入口与快速搭建脚本
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
| Unity 手部输入 | `unity/My project/Assets/Scripts/Common/HandInput.cs` | 接收 21 点手部数据并计算捏合强度 |
| Unity 虚拟手显示 | `unity/My project/Assets/Scripts/Common/HandVisual.cs` | 显示 21 点虚拟手 |
| Unity 抓取逻辑 | `unity/My project/Assets/Scripts/Common/GraspController.cs` | 判断靠近、抓取、移动、释放 |
| Unity 当前场景入口 | `unity/My project/Assets/Scripts/Demo/SceneBootstrap.cs` | 自动搭建训练场景 |
| Unity 训练菜单 | `unity/My project/Assets/Scripts/Training/TrainingMenu.cs` | 显示训练大厅并切换不同训练模块 |
| Unity 分拣训练任务 | `unity/My project/Assets/Scripts/Scenes/ConveyorSorting/FreeMoveTask.cs` | 生成传送带、随机物料、分拣箱、确认按钮与状态面板 |
| Unity 抓取放置任务 | `unity/My project/Assets/Scripts/Scenes/PickPlace/PickPlaceTask.cs` | 独立训练工件抓取、移动和放置 |
| Unity 按钮点击任务 | `unity/My project/Assets/Scripts/Scenes/ButtonPress/ButtonPressTrainingTask.cs` | 独立训练食指点击按钮的稳定触发 |
| Unity 阀门训练任务 | `unity/My project/Assets/Scripts/Scenes/ValveRotation/ValveRotateTrainingTask.cs` | 独立训练阀门旋转与目标角度控制 |
| Unity 推拉开关任务 | `unity/My project/Assets/Scripts/Scenes/ElectricSwitch/ElectricSwitchTask.cs` | 独立训练横杆式推拉开关的上推/下拉操作 |
| Unity 横向滑块任务 | `unity/My project/Assets/Scripts/Scenes/LinearSlider/LinearSliderTrainingTask.cs` | 独立训练滑块沿轨道精确移动到目标刻度 |
| Unity 急停复位任务 | `unity/My project/Assets/Scripts/Scenes/EmergencyStop/EmergencyStopTrainingTask.cs` | 独立训练急停按钮按下锁定与旋转复位 |
| Unity 档位旋钮任务 | `unity/My project/Assets/Scripts/Scenes/ModeSelector/ModeSelectorTrainingTask.cs` | 独立训练 OFF/手动/自动/复位等离散档位选择 |
| Unity 螺栓拧紧任务 | `unity/My project/Assets/Scripts/Scenes/BoltTightening/BoltTighteningTrainingTask.cs` | 独立训练螺栓顺时针拧紧、扭矩进度和达标判断 |

## 2. 当前已实现功能

当前版本已经实现了一个基于真实手势控制的工厂传送带分拣训练原型，核心功能如下：

| 功能模块 | 实现内容 |
|---|---|
| 真实手势识别 | Python 调用 MediaPipe 识别手部 21 个关键点，并通过 WebSocket 实时发送给 Unity |
| Unity 虚拟手 | Unity 根据手部关键点生成可视化虚拟手，显示手掌、手指骨架和抓取光标 |
| 捏合抓取 | 通过拇指和食指距离计算捏合强度，实现靠近物料、捏合抓取、移动搬运、松手释放 |
| 传送带场景 | Unity 自动生成移动传送带、滚筒、护栏和动态条纹，物料会沿传送带持续移动 |
| 随机物料刷新 | 场上同时存在多个物料，物料完成分拣、漏拣或投错后会随机变成红、蓝、绿任意颜色并重新进入传送带 |
| 分拣箱计数 | 底部设置红、蓝、绿三个分拣箱，箱体更大，并在箱内实时显示累计分拣数量 |
| 投放判定 | 物料放入对应颜色分拣箱后自动计数；投错或漏拣会计入失误 |
| 点击按钮 | 使用食指点击逻辑完成确认/重启操作，靠近时显示引导线，手指放到按钮上后轻点即可触发 |
| 阀门操作 | 在独立“阀门旋转”模块中设置调节轮，支持手势旋转、角度进度和靠近引导线 |
| 推拉开关 | 集成同学分支中的推拉开关逻辑，捏合横杆后上下拖动，松手后自动吸附到 ON/OFF 档位 |
| 横向滑块 | 捏合滑块沿水平轨道拖动到目标刻度，训练线性精细调节能力 |
| 急停复位 | 模拟红色急停按钮，支持向下按下锁定、报警灯提示、捏合旋转复位 |
| 档位旋钮 | 捏合旋钮旋转到指定离散档位，训练设备模式选择操作 |
| 螺栓拧紧 | 依次捏合目标螺栓并顺时针旋转，进度满后判定扭矩达标 |
| 状态与评分 | 右侧状态栏显示当前工位阶段、累计分拣数、各箱数量、皮带速度、误投/漏拣数和正确率 |
| 训练菜单 | 启动后先进入训练大厅，可点击进入传送带分拣、抓取放置、按钮点击、阀门旋转、推拉开关、横向滑块、急停复位、档位旋钮、螺栓拧紧等基础模块 |

## 3. 首次环境配置

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

## 4. 启动 Python 手势服务

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

## 5. 启动 Unity 端

1. 打开 Unity Hub。
2. 选择“添加/打开项目”。
3. 打开以下目录：

```text
E:\Desktop\慧动手\HuiDongShou-MVP\unity\My project
```

4. 等待 Unity 编译完成。
5. 确认 Python 手势服务已经启动。
6. 在 Unity 顶部点击 Play。

当前 Unity 会先进入训练大厅，标题为：

```text
慧动手员工培训系统
```

画面中应出现虚拟手和九个训练入口按钮：

```text
传送带分拣 / 抓取放置 / 按钮点击 / 阀门旋转 / 推拉开关 / 横向滑块 / 急停复位 / 档位旋钮 / 螺栓拧紧
```

用食指下点对应按钮即可进入训练模块。进入任务后，左上角会出现“返回菜单”按钮，可随时回到训练大厅。
按钮靠近时会出现从食指到按钮的引导线，但只有手指放在按钮上后做一次向下轻点才会执行切换或确认。

## 6. 当前交互逻辑

当前版本的主流程是“真实手势控制传送带分拣”：

```text
摄像头画面
  -> Python + MediaPipe 识别 21 个手部关键点
  -> ws://127.0.0.1:8765 推送给 Unity
  -> Unity 显示虚拟手
  -> Unity 判断拇指和食指捏合
  -> 靠近传送带物料并持续捏合后抓取
  -> 将物料移动到对应颜色分拣箱
  -> 松手后自动吸附落位并累计计数
  -> 漏拣/投错后计入失误并随机刷新新物料
```

当前关键阈值：

| 参数 | 当前值 | 含义 |
|---|---:|---|
| 抓取阈值 | `0.6` | 捏合强度超过该值才可能抓取 |
| 释放阈值 | `0.4` | 捏合强度低于该值后释放 |
| 抓取确认时间 | `0.06s` | 避免扫过物料时误抓 |
| 释放确认时间 | `0.08s` | 避免手势抖动导致误释放 |
| 同屏物料数 | `6` | 传送带上循环运行的物料数量 |
| 班次目标数 | `10` | 累计分拣到该数量后可以点击确认 |

## 7. WebSocket 端口

| 端口 | 数据内容 | 当前用途 |
|---|---|---|
| `8765` | 原始 21 点手部关键点 | 当前 Unity 主流程使用 |
| `8766` | 高层手势事件 | 预留给阀门、按钮、复杂任务 |

当前 Unity 抓取主要依赖 `8765`。也就是说，Python 只负责识别手部关键点，抓取判断主要在 Unity 中执行。

## 8. 常见问题

### 8.1 Unity 提示连接失败

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

### 8.2 Python 预览窗口黑屏

可能原因：

- 摄像头被微信、浏览器、会议软件占用。
- Windows 摄像头权限没有打开。
- 摄像头编号不是 `0`。
- 光线太暗。

可以先关闭其他占用摄像头的软件，再重新运行：

```powershell
.\scripts\run_camera.ps1
```

### 8.3 Python 能识别手，但 Unity 没有虚拟手

检查顺序：

1. Unity 是否打开的是 `unity\My project`。
2. Python 服务是否还在运行。
3. Unity Console 是否有红色报错。
4. 是否在 Unity 中点击了 Play。

### 8.4 没有抓取成功

看 Unity 左上角调试文字：

```text
抓取判定: 当前值 / 0.58
捏合强度
握拳强度 (不参与)
```

只有抓取判定超过抓取阈值，并且手部光标靠近物料，才会抓住。

### 8.5 没抓住但物体移动

当前自由抓取模式已经关闭手部物理碰撞，理论上不会再被虚拟手直接推走。如果仍然出现该问题，优先检查 Unity Console 是否有旧脚本残留或红色报错。

## 9. 推荐开发顺序

当前已经完成“真实手势识别 + Unity 传送带分拣训练”的 MVP 原型。后续建议按以下顺序继续开发：

1. 手势校准：根据不同用户手型自动设置捏合阈值。
2. 训练记录：保存每次训练的分拣数量、漏拣次数、正确率和用时。
3. 结果页：完成班次确认后展示训练总结。
4. 场景美术优化：将方块替换为更真实的工件、包装盒或零件模型。
5. 任务扩展：增加故障件剔除、重量分拣、危险品识别等训练任务。
6. 数据看板：统计多次训练成绩，用于员工培训评估。
