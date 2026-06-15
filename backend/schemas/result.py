# Unity 训练结果上报请求体
from pydantic import BaseModel


class ResultSubmit(BaseModel):
    """训练结果: Unity 客户端完成训练后上报"""
    student_id: int
    assignment_id: int
    task_id: int
    score: int         # 训练成绩
    train_time: int    # 训练用时（秒）
