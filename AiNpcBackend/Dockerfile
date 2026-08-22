#Base lightweight Python image
FROM python:3.10-slim

#Audio processing (speech/tts) system dependencies
RUN apt-get update && apt-get install -y --no-install-recommends \
    ffmpeg \
    build-essential \
    && rm -rf /var/lib/apt/lists/*


#Working directory defined 
WORKDIR /app

#Requirements being copy and install first (For Docker caching )
COPY  requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

#Project files copy
COPY . .

# Fastapi default port expose 
EXPOSE 8000

# Server start command
CMD ["uvicorn", "main:app","--host","0.0.0.0","--port", "8000"]