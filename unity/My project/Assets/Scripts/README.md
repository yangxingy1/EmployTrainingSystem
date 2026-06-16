# Unity 脚本目录说明

| Folder | Purpose |
|---|---|
| `Core/` | WebSocket 消息模型、接收器等核心通信代码 |
| `Common/` | 手部输入、虚拟手显示、抓取控制、可抓取物和吸附区等通用基础能力 |
| `Interaction/` | 阀门、按钮、工具、工件等手势驱动的交互对象 |
| `Training/` | 训练流程、评分、操作记录、结果判定 |
| `Scenes/` | 传送带分拣、抓取放置、阀门旋转等基础训练场景，每个功能一个子目录 |
| `Demo/` | 快速测试用的场景搭建脚本 |

推荐下一步：

1. 先运行 Python 回放模式，确认 WebSocket 数据可以正常发送。
2. 打开 Unity，把 `Demo/SceneBootstrap.cs` 挂到空物体上。
3. 确认手势事件能触发阀门旋转和按钮点击。
4. 再逐步把示例几何体替换成真实的工厂训练模型。

新增场景建议放在 `Scenes/新场景名/` 下，并在 `Demo/SceneBootstrap.cs` 与 `Training/TrainingMenu.cs` 中注册入口按钮。
