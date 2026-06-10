from __future__ import annotations

import json
from pathlib import Path
from typing import Any


class JsonlRecorder:
    def __init__(self, path: Path | None):
        self.path = path
        self._file = None
        if path:
            path.parent.mkdir(parents=True, exist_ok=True)
            self._file = path.open("a", encoding="utf-8")

    def write(self, kind: str, payload: dict[str, Any]) -> None:
        if not self._file:
            return
        self._file.write(json.dumps({"kind": kind, "payload": payload}, ensure_ascii=False) + "\n")
        self._file.flush()

    def close(self) -> None:
        if self._file:
            self._file.close()

