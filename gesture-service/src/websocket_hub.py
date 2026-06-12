from __future__ import annotations

import asyncio
import json
from typing import Any

import websockets
from websockets.server import WebSocketServerProtocol


class WebSocketHub:
    def __init__(self, host: str, port: int, name: str):
        self.host = host
        self.port = port
        self.name = name
        self._clients: set[WebSocketServerProtocol] = set()
        self._server = None

    async def start(self) -> None:
        self._server = await websockets.serve(self._handler, self.host, self.port)
        print(f"[{self.name}] listening on ws://{self.host}:{self.port}")

    async def _handler(self, websocket: WebSocketServerProtocol):
        self._clients.add(websocket)
        print(f"[{self.name}] client connected, total={len(self._clients)}")
        try:
            await websocket.wait_closed()
        finally:
            self._clients.discard(websocket)
            print(f"[{self.name}] client disconnected, total={len(self._clients)}")

    async def broadcast(self, message: dict[str, Any]) -> None:
        if not self._clients:
            return
        payload = json.dumps(message, ensure_ascii=False)
        dead = []
        for client in list(self._clients):
            try:
                await client.send(payload)
            except Exception:
                dead.append(client)
        for client in dead:
            self._clients.discard(client)

