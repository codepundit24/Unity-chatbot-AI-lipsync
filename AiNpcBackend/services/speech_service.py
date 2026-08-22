import io
import os
from dotenv import load_dotenv
from openai import OpenAI
from utils.logger import get_logger

load_dotenv()
logger = get_logger(__name__)

client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))


async def transcribe_audio(file):
    try:
        logger.info("Sending audio to whisper ")

        audio_bytes = await file.read()

        # Debug ke liye file write
        with open("received_audio.wav", "wb") as w:
            w.write(audio_bytes)

        logger.info(f"Received audio size: {len(audio_bytes)} bytes")

        # Byte stream banana OpenAI API ke liye
        audio_stream = io.BytesIO(audio_bytes)
        audio_stream.name = getattr(file, "filename", "audio.wav")

        # translations.create ki jagah transcriptions.create use karein
        response = client.audio.transcriptions.create(
            model="whisper-1",
            file=audio_stream,
            temperature=0.0,
            prompt="Hello, this is a clear voice recording for conversational chat.",
        )

        return response.text

    except Exception as e:
        logger.error(f"whisper error: {str(e)}")
        raise