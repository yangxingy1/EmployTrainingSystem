# Unity Scripts 目录说明

当前脚本按“公共能力”和“具体训练场景”拆分。

| Folder | Purpose |
|---|---|
| `Core/` | WebSocket 消息模型和接收器。 |
| `Demo/` | 演示入口和通用调试脚本，例如 `SceneBootstrap`。 |
| `Interaction/` | 通用交互示例和接口，例如按钮、旧阀门接口。 |
| `Prototype/` | 手部输入、虚拟手显示、旧抓取放置原型。 |
| `Training/` | 训练流程、操作日志、评分等业务层脚本。 |
| `ElectricSwitch/` | 拉杆式电闸场景专属脚本。 |
| `RotaryValve/` | 旋转阀门场景专属脚本。 |

## 当前场景切换

`Demo/SceneBootstrap.cs` 负责运行时创建训练场景。

在 Inspector 中修改 `sceneKind`：

- `ElectricSwitch`：拉杆式电闸。
- `RotaryValve`：旋转型阀门，红色手轮 + 黄色管道。

默认值目前是 `RotaryValve`。
