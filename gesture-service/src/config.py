from dataclasses import dataclass


@dataclass
class ServiceConfig:
    camera_index: int = 0
    raw_host: str = "localhost"
    raw_port: int = 8765
    event_host: str = "localhost"
    event_port: int = 8766
    target_fps: int = 30
    grab_start_threshold: float = 0.45
    grab_end_threshold: float = 0.25
    click_threshold: float = 0.70
    click_cooldown_s: float = 0.35
