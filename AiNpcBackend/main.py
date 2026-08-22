from fastapi import FastAPI , HTTPException,UploadFile,File
from models.chat_models import ChatRequest, ChatResponse
from services.llm_service import generate_reply
from utils.logger import get_logger
from services.speech_service import transcribe_audio
from services.tts_service import text_to_speech
from fastapi.responses import StreamingResponse
from fastapi.middleware.cors import CORSMiddleware
import io
import base64
app = FastAPI()

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], #for Development
    allow_credentials=False,
    allow_methods=["*"],
    allow_headers=["*"], 
)
logger = get_logger(__name__)

@app.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest):
    logger.info(f"incoming message : {request.message}")

    try:
        reply = await generate_reply(request.message,user_id="Player1")
        logger.info(f"reply generated successfully")
        return ChatResponse(message=reply)
    except Exception :
        logger.error(f"Error occurred while generating reply")
        raise HTTPException(status_code=500, detail="Error occurred while generating reply")


@app.post("/voice_chat")
async def voice_chat(file: UploadFile = File(...)):
    logger.info("voice file received")

    try:
        #speech to text
        user_text = await  transcribe_audio(file)
        logger.info(f"Transcribed text : {user_text}")

        #Send llm with memory
        reply = await generate_reply(user_text,user_id="Player1")

        #Decide gesture
        gesture ="none"

        reply_lower = reply.lower()

        if(
            "hail" in reply_lower
            or"greetings" in reply_lower
        ):
            gesture = "greeting"
        elif(
            "see you" in reply_lower
        ):
            gesture = "goodbye"

        logger.info(f"Selected gesture :{gesture}")

        #Convert reply to speech
        audio_bytes = await text_to_speech(reply)

        audio_base64 = base64.b64encode(audio_bytes).decode("utf-8")

        return {
           "user_text":user_text,
           "message":reply,
           "gesture": gesture,
           "audio_base64":audio_base64
       }

    except Exception as e:
        logger.error(f"Error occurred while generating reply : {str(e)}")
        raise HTTPException(status_code=500,detail="Voice processing failed")