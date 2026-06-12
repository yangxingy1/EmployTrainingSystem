# 系统架构

## MVP 目标

MVP 阶段优先证明一条完整、稳定、可演示的训练闭环：

```text
摄像头手部跟踪
-> 手势识别
-> Unity 虚拟设备响应
-> 训练事件记录
-> 得分与结果展示
```

## 模块划分

| Module | Responsibility |
|---|---|
| Python `gesture-service` | 采集摄像头画面，识别手部关键点，生成手势事件，并通过 WebSocket 广播 |
| Unity `HandInput` | 接收原始 21 点手部关键点，用于虚拟手部显示和旧版物理抓取原型 |
| Unity `GestureReceiver` | 接收语义手势事件，供训练任务直接使用 |
| Unity 交互对象 | 响应手势事件，例如阀门旋转、按钮点击、工件拿取 |
| 训练任务管理器 | 管理引导、练习、考核流程，并记录操作日志 |

## 为什么拆成两个 WebSocket 数据流

已有 Unity 代码期望接收如下原始手部数据：

```json
{"present": true, "x": [0.1], "y": [0.2], "z": [0.0]}
```

如果把“抓取、旋转、点击”这类语义事件混在同一个端口里，旧版 `HandInput.cs` 解析时可能会把它们当成错误数据。因此当前骨架拆成两个端口：

| Stream | URL | Message |
|---|---|---|
| 原始手部数据 | `ws://localhost:8765` | `present + x/y/z arrays` |
| 手势事件数据 | `ws://localhost:8766` | `type=gesture + gesture/state/params` |

这样做的好处是：旧原型可以继续使用，新训练业务也可以直接订阅高层事件，后续扩展时不会互相影响。
