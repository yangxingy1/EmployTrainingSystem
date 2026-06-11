# 目录结构规范

本文档规定“慧动手”MVP 阶段的目录结构。后续开发时，尽量不要把 Unity 脚本、Python 脚本、课程文档和运行数据混放在一起，否则项目后期会很难维护，也不方便展示给老师检查。

```text
HuiDongShou-MVP/
├─ gesture-service/
│  ├─ src/                  Python 源码
│  ├─ samples/              手势回放样例，用于没有摄像头时演示
│  ├─ recordings/           运行时录制数据，默认不提交到 Git
│  ├─ tests/                Python 单元测试或算法测试
│  ├─ requirements.txt      Python 依赖
│  └─ README.md
├─ unity/
│  └─ My project/
│     ├─ Assets/
│     │  ├─ Scenes/         Unity 场景文件
│     │  └─ Scripts/
│     │     ├─ Core/        WebSocket 接收、消息模型、系统核心能力
│     │     ├─ Common/      手部输入、虚拟手、抓取、吸附等通用操作能力
│     │     ├─ Interaction/ 阀门、按钮、零件等可交互对象
│     │     ├─ Training/    训练流程、评分、日志
│     │     ├─ Scenes/      各个基础训练场景，每个场景独立一个子目录
│     │     ├─ Demo/        快速搭建演示场景的脚本
│     │     └─ README.md    Unity 脚本目录说明
│     ├─ Packages/
│     └─ ProjectSettings/
├─ unity-client/            早期 Unity 脚本骨架备份，后续不作为主工程
├─ docs/
│  ├─ architecture.md
│  ├─ websocket-protocol.md
│  └─ directory-structure.md
├─ scripts/
│  ├─ setup_python.ps1
│  ├─ run_camera.ps1
│  └─ run_replay.ps1
└─ README.md
```

## 各目录职责

| 目录 | 作用 | 后续开发建议 |
|---|---|---|
| `gesture-service/src/` | Python 手势识别核心代码 | 手势算法、WebSocket 服务、回放、录制都放这里 |
| `gesture-service/samples/` | 演示样例数据 | 放少量可提交的样例，不放大文件 |
| `gesture-service/recordings/` | 运行时录制数据 | 仅本地调试用，默认不提交 |
| `unity/My project/Assets/Scripts/Core/` | Unity 核心通信层 | 只放通用接收器、消息结构、系统基础能力 |
| `unity/My project/Assets/Scripts/Common/` | Unity 通用交互基础 | 放手部输入、虚拟手、抓取控制、可抓取物、吸附区等通用脚本 |
| `unity/My project/Assets/Scripts/Interaction/` | 场景交互对象 | 阀门、按钮、工具、工件等脚本放这里 |
| `unity/My project/Assets/Scripts/Training/` | 训练业务逻辑 | 任务步骤、评分、日志、报告生成放这里 |
| `unity/My project/Assets/Scripts/Scenes/` | 基础训练场景 | 每个操作场景单独建子目录，例如 `ConveyorSorting/`、`ValveRotation/` |
| `unity/My project/Assets/Scripts/Demo/` | 演示和快速验证 | 放自动搭建场景的脚本，方便答辩展示 |
| `docs/` | 技术文档 | 架构、接口协议、运行说明、目录规范都放这里 |
| `scripts/` | 常用脚本 | 安装依赖、启动服务、回放测试等命令入口 |

## 开发规则

1. Unity 新脚本必须放在 `unity/My project/Assets/Scripts/` 下对应分类目录中，不再放到根目录 `源码/`。
2. Python 新功能必须放在 `gesture-service/src/`，不要把实验脚本散落在桌面。
3. 运行产生的日志、录制数据、Unity 临时文件不提交到 Git。
4. 课程报告、PPT、团队文档保留在根目录或课程资料文件夹中，不参与程序运行。
5. 根目录 `源码/` 只作为旧版参考，正式修改请在 `unity/My project/Assets/Scripts/Common/`、`Interaction/`、`Training/` 或 `Scenes/` 中完成。
6. 新增基础训练时，在 `Scenes/` 下新建独立子目录，并将该模块的入口脚本、辅助脚本和 README 说明放在同一目录。

## 推荐命名

| 类型 | 命名示例 | 说明 |
|---|---|---|
| Unity 场景 | `TrainingWorkshop.unity` | 表达训练场景用途 |
| Unity 脚本 | `ValveInteractable.cs` | 类名与文件名保持一致 |
| Python 模块 | `recognizer.py` | 使用小写加下划线风格 |
| 回放样例 | `pinch_rotate_sample.jsonl` | 说明手势内容 |
| 文档 | `websocket-protocol.md` | 用英文短横线，便于版本管理 |
