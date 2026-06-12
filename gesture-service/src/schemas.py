from __future__ import annotations

import time
from dataclasses import dataclass, field
from typing import Any


@dataclass
class Landmark:
    x: float
    y: float
    z: float


def raw_hand_message(present: bool, landmarks: list[Landmark] | None) -> dict[str, Any]:
    if not present or not landmarks:
        return {"present": False, "x": [], "y": [], "z": []}
    return {
        "present": True,
        "x": [p.x for p in landmarks],
        "y": [p.y for p in landmarks],
        "z": [p.z for p in landmarks],
    }


@dataclass
class GestureEvent:
    seq: int
    hand: str
    gesture: str
    state: str
    confidence: float
    params: dict[str, float] = field(default_factory=dict)
    timestamp: float = field(default_factory=time.time)

    def to_json_dict(self) -> dict[str, Any]:
        return {
            "type": "gesture",
            "seq": self.seq,
            "timestamp": self.timestamp,
            "hand": self.hand,
            "gesture": self.gesture,
            "state": self.state,
            "confidence": self.confidence,
            "params": self.params,
        }

