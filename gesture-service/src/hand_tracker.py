from __future__ import annotations

from typing import Optional

from .schemas import Landmark


class HandTracker:
    """Small wrapper around MediaPipe Hands.

    It returns normalized 21-point landmarks and a handedness label.
    """

    def __init__(self):
        try:
            import mediapipe as mp
        except ImportError as exc:
            raise RuntimeError(
                "mediapipe is not installed. Run: pip install -r requirements.txt"
            ) from exc

        self._mp = mp
        self._hands = mp.solutions.hands.Hands(
            static_image_mode=False,
            max_num_hands=1,
            model_complexity=1,
            min_detection_confidence=0.6,
            min_tracking_confidence=0.5,
        )

    def process_bgr(self, frame_bgr) -> tuple[bool, Optional[list[Landmark]], str]:
        import cv2

        rgb = cv2.cvtColor(frame_bgr, cv2.COLOR_BGR2RGB)
        result = self._hands.process(rgb)
        if not result.multi_hand_landmarks:
            return False, None, "unknown"

        hand_landmarks = result.multi_hand_landmarks[0]
        handedness = "unknown"
        if result.multi_handedness:
            handedness = result.multi_handedness[0].classification[0].label.lower()

        points = [
            Landmark(lm.x, lm.y, lm.z)
            for lm in hand_landmarks.landmark
        ]
        return True, points, handedness

    def close(self) -> None:
        self._hands.close()

