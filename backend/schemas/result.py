from pydantic import BaseModel


class ResultSubmit(BaseModel):

    student_id: int

    assignment_id: int

    task_id: int

    score: int

    train_time: int