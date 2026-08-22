from pydantic import BaseModel
from fastapi import UploadFile
class ChatRequest(BaseModel):
    message : str

class ChatResponse(BaseModel):
    message: str