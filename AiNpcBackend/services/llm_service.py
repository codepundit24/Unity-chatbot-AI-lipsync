from venv import create

from openai import OpenAI
import os
from dotenv import load_dotenv
from pyexpat.errors import messages

from utils.logger import get_logger

conversation_store = {}
load_dotenv()

logger = get_logger(__name__)

client = OpenAI(api_key=os.getenv("OPENAI_API_KEY"))

SYSTEM_PROMPT = """
You are Caleb, a friendly, energetic and intelligent AI assistant.

Your personality:
- Warm and cheerful.
- Speak naturally.
- Sound confident but humble.
- Keep responses conversational.
- Smile through your words.
- Avoid robotic wording.
- Keep answers concise unless asked for details.
- If you don't know something, admit it honestly.
- Do not use emojis in your responses.

Never say you are just an AI unless someone specifically asks.
"""

async def generate_reply(user_message: str,user_id:str="default")-> str:
   try:
       logger.info("Calling Open Api ")

       #Get conversation history or create new
       if user_id not in conversation_store:
           conversation_store[user_id]=[
               {"role":"system","content":SYSTEM_PROMPT}
           ]
       #add user message
       conversation_store[user_id].append(
           {"role":"user","content":user_message}
       )
       response = client.chat.completions.create(
           model="gpt-4o-mini",
           messages = conversation_store[user_id],
           temperature=0.7
       )
       reply = response.choices[0].message.content

       #Add assistant reply to memory
       conversation_store[user_id].append(
           {"role":"assistant","content":reply}
       )

       logger.info("Received response from Open Api")
       return reply
   except Exception as e:
       logger.error(f"LLM error:{str(e)}")
       raise