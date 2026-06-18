ALLOWED_SCENES = {
    "lead-train1": "lead-train1",
    "train2": "train2",
}

LEAD_TRAIN1_SUB_ITEMS = [
    {
        "sub_task_id": "lead_train1_electrical_cabinet_gesture",
        "name": "配电柜主断路器",
        "description": "完成配电柜主断路器确认与操作。",
        "expected_steps": [
            {"index": 0, "name": "主断路器操作", "expectedAction": "Grab + Rotate"}
        ],
    },
    {
        "sub_task_id": "lead_train1_gesture",
        "name": "Breaker Shutdown",
        "description": "按指定顺序完成 4 个电闸的关闭。",
        "expected_steps": [
            {"index": 0, "name": "电闸 1", "expectedAction": "Grab + Slide"},
            {"index": 1, "name": "电闸 2", "expectedAction": "Grab + Slide"},
            {"index": 2, "name": "电闸 3", "expectedAction": "Grab + Slide"},
            {"index": 3, "name": "电闸 4", "expectedAction": "Grab + Slide"},
        ],
    },
    {
        "sub_task_id": "lead_train1_cnc_gesture",
        "name": "CNC 标准加工",
        "description": "使用双层状态机完成 CNC 8 步标准训练。",
        "expected_steps": [
            {"index": 0, "name": "电源上电", "expectedAction": "Point + Tap"},
            {"index": 1, "name": "AUTO 模式确认", "expectedAction": "Grab + Rotate"},
            {"index": 2, "name": "打开安全门", "expectedAction": "Grab + Slide"},
            {"index": 3, "name": "夹紧工件", "expectedAction": "Grab + Rotate"},
            {"index": 4, "name": "关闭安全门", "expectedAction": "Grab + Slide"},
            {"index": 5, "name": "启动自动加工", "expectedAction": "Point + Tap"},
            {"index": 6, "name": "急停", "expectedAction": "Click"},
            {"index": 7, "name": "复位", "expectedAction": "Point + Tap"},
        ],
    },
]

SCENE_SUB_ITEMS = {
    "lead-train1": LEAD_TRAIN1_SUB_ITEMS,
    "train2": [],
}


def normalize_scene_name(scene_name: str | None) -> str:
    return (scene_name or "").strip()


def is_allowed_scene(scene_name: str | None) -> bool:
    return normalize_scene_name(scene_name) in ALLOWED_SCENES


def get_sub_items(scene_name: str | None) -> list[dict]:
    return SCENE_SUB_ITEMS.get(normalize_scene_name(scene_name), [])


def find_sub_item(scene_name: str | None, sub_task_id: str | None) -> dict | None:
    clean_id = (sub_task_id or "").strip()
    for item in get_sub_items(scene_name):
        if item["sub_task_id"] == clean_id:
            return item
    return None


def sub_task_display_name(scene_name: str | None, sub_task_id: str | None) -> str:
    item = find_sub_item(scene_name, sub_task_id)
    if item:
        return item["name"]
    return (sub_task_id or "未命名子项目").strip()
