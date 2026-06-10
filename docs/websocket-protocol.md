# WebSocket 通信协议

## 原始手部关键点数据流

地址：

```text
ws://localhost:8765
```

消息格式：

```json
{
  "present": true,
  "x": [0.0, 0.0, 0.0],
  "y": [0.0, 0.0, 0.0],
  "z": [0.0, 0.0, 0.0]
}
```

`x`、`y`、`z` 必须各包含 21 个数值，对应 MediaPipe Hands 的 21 个手部关键点。

## 语义手势事件数据流

地址：

```text
ws://localhost:8766
```

消息格式：

```json
{
  "type": "gesture",
  "seq": 1,
  "timestamp": 1710000000.0,
  "hand": "right",
  "gesture": "rotate",
  "state": "update",
  "confidence": 0.92,
  "params": {
    "x": 0.52,
    "y": 0.43,
    "pinchStrength": 0.8,
    "angleDelta": 6.5,
    "totalAngle": 42.0
  }
}
```

## 手势名称

| Gesture | State | Meaning |
|---|---|---|
| `grab` | `start`, `update`, `end` | 捏合或抓取状态 |
| `rotate` | `update` | 抓取状态下的旋转角度变化 |
| `click` | `trigger` | 按钮点击类动作 |
| `hand` | `lost` | 手部丢失或未检测到 |

## Unity 对接建议

| 使用场景 | 推荐监听端口 |
|---|---|
| 显示虚拟手部骨骼 | `ws://localhost:8765` |
| 做抓取方块等物理原型 | `ws://localhost:8765` |
| 控制阀门、按钮、设备部件 | `ws://localhost:8766` |
| 训练步骤判定和评分 | `ws://localhost:8766` |
