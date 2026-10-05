# 🌐 NeuralWorld.Space — Live Speech Translator

> Real-time speech translation with a privacy-first, browser-based AI pipeline.

[![Live Demo](https://img.shields.io/badge/Live%20Demo-neuralworld.space-00f2fe?style=for-the-badge)](https://neuralworld.space/)
[![.NET](https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![SignalR](https://img.shields.io/badge/SignalR-Realtime-512BD4?style=for-the-badge&logo=signalr)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![JavaScript](https://img.shields.io/badge/JavaScript-Browser%20AI-F7DF1E?style=for-the-badge&logo=javascript&logoColor=000)](https://developer.mozilla.org/en-US/docs/Web/JavaScript)

## 🚀 Live Application

### 👉 **https://neuralworld.space/**

NeuralWorld Live Speech Translator is a real-time voice translation application designed around a privacy-first browser experience.

Speak into the microphone, release the push-to-talk control, and the application:

**Speech → Text → Translation → Voice**

The translation stage runs in the browser using **Transformers.js** and quantized translation models, with browser caching so models do not need to be downloaded again on every visit.

---

## ✨ Features

- 🎙️ **Push-to-talk speech recognition**
- ⚡ **Real-time translation workflow**
- 🧠 **Browser-side AI translation**
- 🔐 **Privacy-focused client-side translation**
- 💾 **Browser model caching**
- 🔊 **Text-to-speech playback**
- 🤖 **Animated AI robot while speaking**
- 📱 **Responsive browser UI**
- 📲 **Android APK support**
- 🔄 **SignalR real-time communication infrastructure**
- 🧩 **ASP.NET Core modular service architecture**
- 🌐 **Multiple language pairs**
- 📊 **Model loading and translation telemetry**

---

## 🌍 Supported Languages

The current UI supports:

| Language | Code |
|---|---|
| Hindi | `hi` |
| English | `en` |
| Spanish | `es` |
| Japanese | `ja` |
| Chinese | `zh` |

The application supports direct English↔language translation and uses a two-step route through English for supported non-English↔non-English combinations.

Examples:

- Hindi → English
- English → Hindi
- Spanish → English
- English → Spanish
- Japanese → English
- English → Japanese
- Chinese → English
- English → Chinese
- Hindi → Spanish
- Japanese → Chinese

---

## 🧠 How It Works

The application is designed as a low-latency speech translation pipeline:

```text
┌──────────────────────┐
│      Microphone      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Browser Speech STT   │
│   Web Speech API     │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Transformers.js      │
│ Quantized ONNX Model │
│ Browser-side AI      │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│   Translation Text   │
└──────────┬───────────┘
           │
           ▼
┌──────────────────────┐
│ Browser Text-to-     │
│ Speech / Android TTS │
└──────────┬───────────┘
           │
           ▼
      🔊 Audio Output
```

---

## 🔒 Privacy

One of the main goals of this project is to keep the translation workload on the user's device.

The web application uses **client-side Transformers.js translation models**. After the required model files are downloaded and cached by the browser, translation can continue using the locally cached model files.

> **Important:** Speech recognition and browser capabilities can vary by browser and platform. The privacy characteristics of microphone transcription depend on the speech-recognition implementation provided by the browser/device.

---

## 🧠 Browser AI Model Architecture

The project uses **Xenova Transformers.js** with quantized translation models.

Current model mappings include:

```text
hi-en  → Xenova/opus-mt-hi-en
en-hi  → Xenova/opus-mt-en-hi

es-en  → Xenova/opus-mt-es-en
en-es  → Xenova/opus-mt-en-es

ja-en  → Xenova/opus-mt-ja-en
en-ja  → Xenova/opus-mt-en-jap

zh-en  → Xenova/opus-mt-zh-en
en-zh  → Xenova/opus-mt-en-zh
```

Models are loaded inside a browser Web Worker to keep model inference work separate from the main UI thread.

---

## 🏗️ Project Architecture

The solution is built with **ASP.NET Core .NET 10**.

### Backend

- ASP.NET Core
- .NET 10
- REST Controllers
- SignalR
- MessagePack protocol
- Dependency Injection
- Microsoft.Extensions.AI
- OllamaSharp
- OpenAI-compatible provider support
- Scalar / OpenAPI development tooling

### Main services

```text
Services/
├── SpeechSessionService.cs
├── SpeechToTextService.cs
├── TextToSpeechService.cs
└── TranslationService.cs
```

### Controllers

```text
Controllers/
├── SpeechToTextController.cs
├── SpeechTranslationController.cs
├── TextToSpeechController.cs
└── TranslationController.cs
```

### Real-time communication

The project contains a SignalR hub:

```text
/hubs/translation
```

implemented by:

```text
Hubs/TranslationHub.cs
```

SignalR is configured with MessagePack and a maximum receive message size of 1 MB.

---

## 🌐 Frontend

The main client is located under:

```text
wwwroot/
├── index.html
├── app.js
├── audio-processor.js
└── style.css
```

The frontend provides:

- language selection
- push-to-talk interaction
- model loading progress
- translation status
- speech logs
- animated waveform
- translation cards
- text-to-speech playback
- animated speaking robot
- Android-aware TTS integration

---

## 🤖 Android Support

The project also includes an Android APK distribution path:

```text
apk/
└── LiveSpeechTranslator.apk
```

The web application exposes the APK folder through ASP.NET Core static-file hosting.

On supported Android deployments, native Android TTS can be used for translated speech playback.

---

## 🛠️ Technology Stack

| Layer | Technology |
|---|---|
| Backend | ASP.NET Core |
| Runtime | .NET 10 |
| Real-time | SignalR |
| Protocol | MessagePack |
| AI abstraction | Microsoft.Extensions.AI |
| Local model integration | OllamaSharp |
| Browser AI | Transformers.js |
| Translation models | Opus-MT / MarianMT family |
| Speech input | Web Speech API |
| Speech output | Browser SpeechSynthesis / Android TTS |
| API documentation | OpenAPI + Scalar |
| Frontend | HTML, CSS, JavaScript |
| Android | Native Android integration |

---

## 📂 Repository Structure

```text
NeuralWorld.Space.LiveSpeech.Translator/
│
├── NeuralWorld.Space.LiveSpeech.Translator.slnx
│
├── NeuralWorld.Space.LiveSpeech.Translator/
│   ├── Controllers/
│   ├── Hubs/
│   ├── Interfaces/
│   ├── Services/
│   ├── Properties/
│   ├── apk/
│   ├── wwwroot/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── appsettings.Production.json
│   └── NeuralWorld.Space.LiveSpeech.Translator.csproj
│
└── README.md
```

---

## ▶️ Running Locally

### Prerequisites

Install:

- .NET 10 SDK
- A modern browser with JavaScript support
- Microphone permission
- Internet access for the initial model download

### Clone

```bash
git clone https://github.com/nirajsony/NeuralWorld.Space.LiveSpeech.Translator.git
cd NeuralWorld.Space.LiveSpeech.Translator
```

### Run

```bash
dotnet restore
dotnet run
```

Then open the HTTPS URL shown by ASP.NET Core.

For browser speech recognition, grant microphone permission when prompted.

---

## ⚙️ Translation Provider Configuration

The backend supports provider-based translation configuration.

The application can be configured for:

- Ollama
- OpenAI-compatible endpoints
- Other compatible providers exposed through the Microsoft.Extensions.AI abstraction

Example configuration shape:

```json
{
  "Translation": {
    "Provider": "Ollama",
    "Endpoint": "http://localhost:11434",
    "Model": "qwen3-translator"
  }
}
```

### ⚠️ Security

**Never commit real API keys or production secrets to GitHub.**

Use environment variables, user secrets, deployment secrets, or your hosting provider's secure configuration system.

---

## 🌟 Design Goals

NeuralWorld Live Speech Translator was created around four core goals:

### 1. Privacy

Keep as much of the AI processing as possible on the user's own device.

### 2. Real-time interaction

Avoid traditional upload → wait → download translation workflows where possible.

### 3. Cross-platform usability

Make the translator usable from desktop browsers, mobile browsers and Android.

### 4. Lightweight AI

Use quantized models and browser caching to make local AI practical on consumer hardware.

---

## 🚧 Beta Status

This project is under active development and is currently a **beta** application.

Possible areas for future improvement include:

- better speech recognition across browsers
- additional languages
- better translation quality for less common language pairs
- lower model startup time
- improved mobile performance
- fully offline speech recognition
- improved Android integration
- more advanced streaming translation

---

## 🧪 Experimental / Research Project

NeuralWorld is also a practical exploration of how modern AI models can be deployed outside traditional cloud-only architectures.

The project combines:

- browser-side AI inference
- quantized ONNX models
- Web Workers
- real-time speech interfaces
- ASP.NET Core
- SignalR
- mobile integration

The goal is to explore practical, privacy-conscious AI experiences that can run on everyday devices.

---

## 👨‍💻 Author

**Niraj Soni**

Built as part of the **NeuralWorld** project.

### 🌐 NeuralWorld

**https://neuralworld.space/**

---

## 📜 License

This repository does not currently specify a license. Unless a license is added to the repository, the default copyright rules apply and reuse of the source code may require permission from the copyright holder.

---

## ⭐ Support the Project

If you find NeuralWorld interesting or useful, consider giving the repository a ⭐ on GitHub and following the project as it evolves.

**Live:** https://neuralworld.space/

**Source:** https://github.com/nirajsony/NeuralWorld.Space.LiveSpeech.Translator
