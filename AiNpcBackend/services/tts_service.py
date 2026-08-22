from http.client import responses

from openai import OpenAI
from dotenv import load_dotenv
import os

from services.llm_service import client
from utils.logger import get_logger

logger = get_logger(__name__)
load_dotenv()

client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

async def text_to_speech(text:str):
    try:
        logger.info("Genrating speech from text")

        response = client.audio.speech.create(
            model="gpt-4o-mini-tts",
            voice="alloy",
            input=text,
            response_format="wav"
        )

        audio_bytes= response.read()

        return audio_bytes
    except Exception as e:
        logger.error(f"TTS error :{str(e)}")
        raise