# AI Chat Bot — Real-Time Conversational NPC (Unity + OVR LipSync)

A Unity-based conversational AI character that listens, thinks, and talks back — with real-time lip-sync driven by **Oculus (Meta) LipSync**, powered by a Python/FastAPI backend integrating **OpenAI GPT-4** and **Whisper STT**.


---

## ✨ Features

- 🎙️ **Speech-to-Text** — real-time transcription using OpenAI Whisper
- 🧠 **LLM-Driven Responses** — conversational replies generated via GPT-4
- 🗣️ **Real-Time Lip-Sync** — Oculus LipSync analyzes generated speech audio and drives viseme-based facial animation in Unity
- ✋ **Hand-Tracking Mic Control** — MediaPipe tracks the user's hand via webcam to trigger/mute the microphone with a gesture, no button press needed
- ⚡ **Low-Latency Pipeline** — end-to-end voice-in → response-out loop optimized for real-time interaction
- ☁️ **Cloud-Hosted Backend** — FastAPI service deployed for speech/LLM processing, decoupled from the Unity client

---

## 🏗️ Architecture

```
Webcam Feed ──► MediaPipe Hand Tracking ──► Gesture Detected ──► Mic Trigger (on/off)
                                                                       │
                                                                       ▼
                                                              User Voice Input
                                                                       │
                                                                       ▼
Unity Client  ──(audio stream)──►  FastAPI Backend
                                        │
                                        ├── Whisper STT (speech → text)
                                        │
                                        ├── GPT-4 (text → response)
                                        │
                                        └── TTS (response → speech audio)
      ▲
      │ (audio response)
      ▼
Oculus LipSync (Unity) ──► Viseme Blend Shapes ──► Character Facial Animation
```

1. **MediaPipe** tracks the user's hand via webcam in real time; a defined gesture toggles the microphone on/off — no UI button needed.
2. Once the mic is active, user speaks → audio captured in Unity and sent to the backend.
3. Backend transcribes speech (Whisper), generates a reply (GPT-4), and synthesizes speech audio (TTS).
4. Unity receives the audio response and feeds it to **OVR LipSync**, which analyzes the waveform in real time and drives the character's viseme blend shapes for accurate mouth movement.

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Game Engine | Unity 3D |
| Facial Animation | Oculus (Meta) LipSync SDK |
| Gesture Control | Google MediaPipe (hand tracking via webcam) |
| Backend | Python, FastAPI |
| Speech-to-Text | OpenAI Whisper |
| Conversational AI | OpenAI GPT-4 |
| Hosting | AWS |

---

## 📂 Project Structure

```
Ai Chat bot/
├── Assets/              # Unity assets, scenes, character, scripts
├── ProjectSettings/     # Unity project configuration
├── Packages/            # Unity Package Manager dependencies
└── ...                  # Standard Unity-generated folders (ignored, see .gitignore)
```

> Note: `Library/`, `Build/`, `Logs/`, `obj/`, `UserSettings/`, `.vs/`, and IDE/project files (`.csproj`, `.sln`) are excluded via `.gitignore` — they're auto-generated and shouldn't be committed.

---

## 🚀 Getting Started

> **Note:** This repository contains the **Unity client only**. The Python/FastAPI backend (Whisper STT + GPT-4 + TTS) is a separate project/service and is not included here — the app expects a running backend instance to connect to.

### Prerequisites
- Unity (version used to build this project — update with your exact version)
- Oculus LipSync SDK (imported into `Assets/`)
- A running instance of the backend service (URL/endpoint needed for config)

### Unity Setup
1. Open the project in Unity.
2. Set the backend endpoint URL in the relevant config script.
3. Ensure the OVR LipSync component is attached to the character's audio source.
4. Grant webcam access for MediaPipe hand tracking (used to trigger the mic).
5. Press Play, show the trigger gesture to activate the mic, and start talking.

---

## 🗺️ Roadmap

- [ ] Improve response latency further
- [ ] Add conversation memory / context persistence
- [ ] Expand emotion-driven animation beyond lip-sync

---

## 📄 License

This project is for personal/portfolio use. Update this section if you plan to open-source it.
