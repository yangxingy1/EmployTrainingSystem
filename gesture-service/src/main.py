from __future__ import annotations

import argparse
import asyncio
from pathlib import Path


def open_camera(cv2, camera_index: int, backend_name: str, width: int, height: int):
    backend_map = {
        "auto": 0,
        "dshow": cv2.CAP_DSHOW,
        "msmf": cv2.CAP_MSMF,
    }
    backend = backend_map.get(backend_name, cv2.CAP_DSHOW)
    cap = cv2.VideoCapture(camera_index, backend) if backend else cv2.VideoCapture(camera_index)
    if cap.isOpened():
        cap.set(cv2.CAP_PROP_FRAME_WIDTH, width)
        cap.set(cv2.CAP_PROP_FRAME_HEIGHT, height)
        cap.set(cv2.CAP_PROP_FPS, 30)
    return cap


HAND_CONNECTIONS = [
    (0, 1), (1, 2), (2, 3), (3, 4),
    (0, 5), (5, 6), (6, 7), (7, 8),
    (5, 9), (9, 10), (10, 11), (11, 12),
    (9, 13), (13, 14), (14, 15), (15, 16),
    (13, 17), (0, 17), (17, 18), (18, 19), (19, 20),
]


def draw_preview(frame, present, points, hand, events) -> None:
    import cv2

    h, w = frame.shape[:2]
    brightness = float(cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY).mean())
    label = "no hand"
    if present and points:
        label = f"hand: {hand}"
        for a, b in HAND_CONNECTIONS:
            pa = points[a]
            pb = points[b]
            cv2.line(
                frame,
                (int(pa.x * w), int(pa.y * h)),
                (int(pb.x * w), int(pb.y * h)),
                (40, 220, 120),
                2,
            )
        for point in points:
            cv2.circle(frame, (int(point.x * w), int(point.y * h)), 4, (40, 160, 255), -1)

    if events:
        last = events[-1]
        label += f" | {last.gesture}:{last.state} {last.confidence:.2f}"
    label += f" | brightness={brightness:.1f}"

    cv2.putText(frame, label, (16, 32), cv2.FONT_HERSHEY_SIMPLEX, 0.8, (20, 240, 240), 2)
    cv2.putText(frame, "Press Q to stop", (16, h - 18), cv2.FONT_HERSHEY_SIMPLEX, 0.65, (230, 230, 230), 2)
    cv2.imshow("HuiDongShou Gesture Preview", frame)


async def run_camera(args) -> None:
    import cv2
    from .config import ServiceConfig
    from .hand_tracker import HandTracker
    from .recognizer import RuleBasedRecognizer
    from .recorder import JsonlRecorder
    from .schemas import raw_hand_message
    from .websocket_hub import WebSocketHub

    config = ServiceConfig(camera_index=args.camera)
    raw_hub = WebSocketHub(config.raw_host, config.raw_port, "raw-hand")
    event_hub = WebSocketHub(config.event_host, config.event_port, "gesture-event")
    await raw_hub.start()
    await event_hub.start()

    tracker = HandTracker()
    recognizer = RuleBasedRecognizer(config)
    recorder = JsonlRecorder(Path(args.record) if args.record else None)
    cap = open_camera(cv2, config.camera_index, args.backend, args.width, args.height)
    if not cap.isOpened():
        raise RuntimeError(f"Cannot open camera index {config.camera_index} with backend {args.backend}")

    delay = 1.0 / max(config.target_fps, 1)
    black_frame_warned = False
    print(f"[main] camera mode started. camera={config.camera_index}, backend={args.backend}. Press Ctrl+C to stop.")
    try:
        while True:
            ok, frame = cap.read()
            if not ok:
                await asyncio.sleep(delay)
                continue
            if not args.no_mirror:
                frame = cv2.flip(frame, 1)

            if args.preview and not black_frame_warned:
                gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)
                if float(gray.mean()) < 18.0:
                    print("[main] warning: camera frame is almost black. Check camera cover, privacy permission, lighting, or try --camera 1 / --backend msmf.")
                    black_frame_warned = True

            present, points, hand = tracker.process_bgr(frame)
            raw = raw_hand_message(present, points)
            await raw_hub.broadcast(raw)
            recorder.write("raw", raw)

            events = recognizer.update(present, points, hand)
            for event in events:
                payload = event.to_json_dict()
                await event_hub.broadcast(payload)
                recorder.write("event", payload)

            if args.preview:
                draw_preview(frame, present, points, hand, events)
                if cv2.waitKey(1) & 0xFF == ord("q"):
                    break

            await asyncio.sleep(delay)
    finally:
        if args.preview:
            cv2.destroyAllWindows()
        recorder.close()
        tracker.close()
        cap.release()


async def run_replay(args) -> None:
    from .config import ServiceConfig
    from .replay import read_replay
    from .websocket_hub import WebSocketHub

    config = ServiceConfig()
    raw_hub = WebSocketHub(config.raw_host, config.raw_port, "raw-hand")
    event_hub = WebSocketHub(config.event_host, config.event_port, "gesture-event")
    await raw_hub.start()
    await event_hub.start()

    rows = list(read_replay(Path(args.replay)))
    if not rows:
        raise RuntimeError("Replay file is empty")
    delay = 1.0 / max(args.fps, 1)
    print(f"[main] replay mode started with {len(rows)} rows.")
    while True:
        for kind, payload in rows:
            if kind == "raw":
                await raw_hub.broadcast(payload)
            elif kind == "event":
                await event_hub.broadcast(payload)
            await asyncio.sleep(delay)


def parse_args():
    parser = argparse.ArgumentParser()
    parser.add_argument("--mode", choices=["camera", "replay"], default="camera")
    parser.add_argument("--camera", type=int, default=0)
    parser.add_argument("--record", default="")
    parser.add_argument("--replay", default="samples/replay_sample.jsonl")
    parser.add_argument("--fps", type=int, default=30)
    parser.add_argument("--preview", action="store_true")
    parser.add_argument("--no-mirror", action="store_true")
    parser.add_argument("--backend", choices=["dshow", "msmf", "auto"], default="dshow")
    parser.add_argument("--width", type=int, default=640)
    parser.add_argument("--height", type=int, default=480)
    return parser.parse_args()


def main() -> None:
    args = parse_args()
    if args.mode == "camera":
        asyncio.run(run_camera(args))
    else:
        asyncio.run(run_replay(args))


if __name__ == "__main__":
    main()
