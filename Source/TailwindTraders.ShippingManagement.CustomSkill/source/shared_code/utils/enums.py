from enum import Enum

class CsResponseStatus(Enum):
    Failed = "Failed"
    NotStarted = "NotStarted"
    Running = "Running"
    Succeeded = "Succeeded"