from __future__ import annotations

import math
import time

from .config import ServiceConfig
from .schemas import GestureEvent, Landmark


class RuleBasedRecognizer:
    """Rule-based MVP recognizer.

    Python keeps recognition simple and explainable:
    - pinch strength from thumb tip to index tip distance
    - grab state with hysteresis
    - rotation from index MCP -> pinky MCP angle while grabbing
    - click from strong pinch with cooldown
    """

    def __init__(self, config: ServiceConfig):
        self.config = config
        self.seq = 0
        self.is_grabbing = False
        self.prev_angle: float | None = None
        self.total_angle = 0.0
        self.last_click_time = -999.0
        self.click_down = False
        self.hand_lost_sent = False

    def update(self, present: bool, points: list[Landmark] | None, hand: str) -> list[GestureEvent]:
        if not present or not points:
            if not self.hand_lost_sent:
                self.hand_lost_sent = True
                return [self._event(hand, "hand", "lost", 0.0, {})]
            self.is_grabbing = False
            self.prev_angle = None
            self.click_down = False
            return []

        self.hand_lost_sent = False
        grip = self._grip_strength(points)
        center = self._center(points)
        angle = self._palm_angle(points)
        events: list[GestureEvent] = []

        if self.prev_angle is not None:
            delta = self._wrap_degrees(angle - self.prev_angle)
            if abs(delta) >= 0.5:
                self.total_angle += delta
                events.append(self._event(hand, "rotate", "update", min(1.0, max(grip, 0.55)), {
                    "angleDelta": delta,
                    "totalAngle": self.total_angle,
                    "x": center[0],
                    "y": center[1],
                    "pinchStrength": grip,
                }))
        self.prev_angle = angle

        if not self.is_grabbing and grip >= self.config.grab_start_threshold:
            self.is_grabbing = True
            self.total_angle = 0.0
            events.append(self._event(hand, "grab", "start", grip, {
                "x": center[0],
                "y": center[1],
                "pinchStrength": grip,
            }))

        if self.is_grabbing:
            events.append(self._event(hand, "grab", "update", grip, {
                "x": center[0],
                "y": center[1],
                "pinchStrength": grip,
            }))

        if self.is_grabbing and grip <= self.config.grab_end_threshold:
            self.is_grabbing = False
            events.append(self._event(hand, "grab", "end", 1.0 - grip, {
                "x": center[0],
                "y": center[1],
                "pinchStrength": grip,
            }))

        now = time.time()
        is_click_down = grip >= self.config.click_threshold
        if is_click_down and not self.click_down and now - self.last_click_time >= self.config.click_cooldown_s:
            self.last_click_time = now
            events.append(self._event(hand, "click", "trigger", grip, {
                "x": points[8].x,
                "y": points[8].y,
                "pinchStrength": grip,
            }))
        self.click_down = is_click_down

        return events

    def _event(self, hand: str, gesture: str, state: str, confidence: float, params: dict[str, float]) -> GestureEvent:
        self.seq += 1
        return GestureEvent(self.seq, hand, gesture, state, confidence, params)

    @staticmethod
    def _distance(a: Landmark, b: Landmark) -> float:
        return math.sqrt((a.x - b.x) ** 2 + (a.y - b.y) ** 2 + (a.z - b.z) ** 2)

    def _pinch_strength(self, points: list[Landmark]) -> float:
        palm_width = max(self._distance(points[5], points[17]), 1e-4)
        ratio = self._distance(points[4], points[8]) / palm_width
        # ratio near 0.35 means closed, near 1.0 means open.
        open_ratio = 1.0
        close_ratio = 0.35
        return max(0.0, min(1.0, (open_ratio - ratio) / (open_ratio - close_ratio)))

    def _fist_strength(self, points: list[Landmark]) -> float:
        palm_width = max(self._distance(points[5], points[17]), 1e-4)
        center = self._center3(points, [0, 5, 9, 13, 17])
        tip_ids = [8, 12, 16, 20]
        avg_tip_distance = sum(self._distance_to_tuple(points[i], center) for i in tip_ids) / len(tip_ids)
        ratio = avg_tip_distance / palm_width
        # Open hands keep fingertips far from the palm center. A fist bends tips inward.
        open_ratio = 1.25
        close_ratio = 0.55
        return max(0.0, min(1.0, (open_ratio - ratio) / (open_ratio - close_ratio)))

    def _grip_strength(self, points: list[Landmark]) -> float:
        return max(self._pinch_strength(points), self._fist_strength(points))

    @staticmethod
    def _center3(points: list[Landmark], ids: list[int]) -> tuple[float, float, float]:
        return (
            sum(points[i].x for i in ids) / len(ids),
            sum(points[i].y for i in ids) / len(ids),
            sum(points[i].z for i in ids) / len(ids),
        )

    @staticmethod
    def _distance_to_tuple(point: Landmark, target: tuple[float, float, float]) -> float:
        return math.sqrt((point.x - target[0]) ** 2 + (point.y - target[1]) ** 2 + (point.z - target[2]) ** 2)

    @staticmethod
    def _center(points: list[Landmark]) -> tuple[float, float]:
        ids = [0, 5, 9, 13, 17]
        return (
            sum(points[i].x for i in ids) / len(ids),
            sum(points[i].y for i in ids) / len(ids),
        )

    @staticmethod
    def _palm_angle(points: list[Landmark]) -> float:
        dx = points[17].x - points[5].x
        dy = points[17].y - points[5].y
        return math.degrees(math.atan2(dy, dx))

    @staticmethod
    def _wrap_degrees(delta: float) -> float:
        while delta > 180.0:
            delta -= 360.0
        while delta < -180.0:
            delta += 360.0
        return delta
