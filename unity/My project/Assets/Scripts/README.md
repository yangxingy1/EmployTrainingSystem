# Unity 脚本目录说明

| Folder | Purpose |
|---|---|
| `Core/` | WebSocket 消息模型、接收器等核心通信代码 |
| `Interaction/` | 阀门、按钮、工具、工件等手势驱动的交互对象 |
| `Training/` | 训练流程、评分、操作记录、结果判定 |
| `Demo/` | 快速测试用的场景搭建脚本 |
| `Prototype/` | 从根目录 `源码/` 复制来的旧版抓取放置原型 |

推荐下一步：

1. 先运行 Python 回放模式，确认 WebSocket 数据可以正常发送。
2. 打开 Unity，把 `Demo/SceneBootstrap.cs` 挂到空物体上。
3. 确认手势事件能触发阀门旋转和按钮点击。
4. 再逐步把示例几何体替换成真实的工厂训练模型。
