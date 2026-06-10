from __future__ import annotations

import json
from pathlib import Path
from typing import Any, Iterator


def read_replay(path: Path) -> Iterator[tuple[str, dict[str, Any]]]:
    with path.open("r", encoding="utf-8") as f:
        for line in f:
            if not line.strip():
                continue
            row = json.loads(line)
            yield row["kind"], row["payload"]

