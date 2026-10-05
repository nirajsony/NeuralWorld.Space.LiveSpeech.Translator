const sourceLangSelect = document.getElementById("sourceLang");
const targetLangSelect = document.getElementById("targetLang");
const chatContainer = document.getElementById("chatContainer");
const emptyMessage = document.getElementById("emptyMessage");

const voiceTitle = document.getElementById("voiceTitle");
const voiceSub = document.getElementById("voiceSub");
const waveform = document.getElementById("waveform");
const micOrb = document.getElementById("micOrb");
const tfjsDot = document.getElementById("tfjsDot");

let activeCard = null;
let isModelLoaded = false;
let isHolding = false;

function appendLog(elementId, text, type = 'normal') {
    const container = document.getElementById(elementId);
    if (!container) return;
    const logEntry = document.createElement("div");
    logEntry.className = `log-entry ${type}`;
    logEntry.textContent = `> ${text}`;
    container.appendChild(logEntry);
    container.scrollTop = container.scrollHeight;
}

const opusMap = {
    "hi": "hi",
    "en": "en",
    "es": "es",
    "ja": "ja",
    "zh": "zh"
};

// -------------------------------------------------------------------
// TRANSFORMERS.JS WEB WORKER LOGIC
// -------------------------------------------------------------------
const workerCode = `
    import { pipeline, env } from 'https://cdn.jsdelivr.net/npm/@xenova/transformers@2.17.2';

    env.allowLocalModels = false;
    env.allowRemoteModels = true;
    env.useBrowserCache = true;

    const models = {};
    const modelPromises = {};

    const modelMap = {
        "hi-en": "Xenova/opus-mt-hi-en",
        "en-hi": "Xenova/opus-mt-en-hi",
        "es-en": "Xenova/opus-mt-es-en",
        "en-es": "Xenova/opus-mt-en-es",
        "ja-en": "Xenova/opus-mt-ja-en",
        "en-ja": "Xenova/opus-mt-en-jap",
        "zh-en": "Xenova/opus-mt-zh-en",
        "en-zh": "Xenova/opus-mt-en-zh"
    };

    async function loadModel(modelId, index, total) {
        if (!modelId) {
            throw new Error("No translation model is configured for this language pair.");
        }

        if (models[modelId]) {
            return models[modelId];
        }

        if (modelPromises[modelId]) {
            return modelPromises[modelId];
        }

        self.postMessage({
            status: 'start_model',
            modelId,
            index,
            total
        });

        modelPromises[modelId] = pipeline(
            'translation',
            modelId,
            {
                quantized: true,
                progress_callback: (data) => {
                    self.postMessage({
                        status: 'progress',
                        modelId,
                        data
                    });
                }
            }
        );

        try {
            const translator = await modelPromises[modelId];
            models[modelId] = translator;

            self.postMessage({
                status: 'model_ready',
                modelId
            });

            return translator;
        } catch (error) {
            delete modelPromises[modelId];
            throw error;
        }
    }

    async function loadRequiredModels(src_lang, tgt_lang) {
        if (src_lang === tgt_lang) {
            return;
        }

        if (src_lang !== 'en' && tgt_lang !== 'en') {
            const m1 = modelMap[\`\${src_lang}-en\`];
            const m2 = modelMap[\`en-\${tgt_lang}\`];

            await loadModel(m1, 1, 2);
            await loadModel(m2, 2, 2);
        } else {
            const modelId = modelMap[\`\${src_lang}-\${tgt_lang}\`];
            await loadModel(modelId, 1, 1);
        }
    }

    self.onmessage = async (event) => {
        const { action, text, src_lang, tgt_lang } = event.data;

        if (action === 'load') {
            try {
                self.postMessage({
                    status: 'cache_info',
                    message: 'Browser model cache is enabled. Existing cached files will be reused.'
                });

                await loadRequiredModels(src_lang, tgt_lang);

                self.postMessage({
                    status: 'all_ready'
                });
            } catch (e) {
                self.postMessage({
                    status: 'error',
                    error: e?.message || String(e)
                });
            }
            return;
        }

        if (action === 'translate') {
            try {
                let resultText = text;

                if (src_lang === tgt_lang) {
                    self.postMessage({
                        status: 'translated',
                        result: text
                    });
                    return;
                }

                if (src_lang !== 'en' && tgt_lang !== 'en') {
                    const model1Id = modelMap[\`\${src_lang}-en\`];
                    const translator1 = await loadModel(model1Id, 1, 2);
                    const res1 = await translator1(resultText);
                    resultText = res1[0].translation_text;

                    const model2Id = modelMap[\`en-\${tgt_lang}\`];
                    const translator2 = await loadModel(model2Id, 2, 2);
                    const res2 = await translator2(resultText);
                    resultText = res2[0].translation_text;
                } else {
                    const modelId = modelMap[\`\${src_lang}-\${tgt_lang}\`];
                    const translator = await loadModel(modelId, 1, 1);
                    const res = await translator(resultText);
                    resultText = res[0].translation_text;
                }

                self.postMessage({
                    status: 'translated',
                    result: resultText
                });
            } catch (e) {
                self.postMessage({
                    status: 'error',
                    error: e?.message || "Translation failed."
                });
            }
        }
    };
`;

const blob = new Blob([workerCode], { type: 'application/javascript' });
const tfWorker = new Worker(URL.createObjectURL(blob), { type: 'module' });

let lastProgress = {};
let currentLoadIndex = 1;
let currentLoadTotal = 1;
let modelLoadStartedAt = 0;

tfWorker.onmessage = (event) => {
    const { status, modelId, index, total, data, result, error, message } = event.data;

    if (status === 'cache_info') {
        appendLog("transformersLogs", message || "Browser cache enabled.", "info");
    } else if (status === 'start_model') {
        currentLoadIndex = index;
        currentLoadTotal = total;
        modelLoadStartedAt = performance.now();

        voiceTitle.textContent = `Loading AI (${index}/${total})...`;
        appendLog("transformersLogs", `Preparing ${modelId}...`, "info");
        appendLog("transformersLogs", "If this model was downloaded before, cached files will be reused.", "info");
    } else if (status === 'progress') {
        if (!data) return;

        if (data.status === 'progress') {
            const progress = Number.isFinite(data.progress) ? Math.round(data.progress) : 0;

            if (data.file && (data.file.endsWith('.onnx') || data.file.endsWith('.bin'))) {
                voiceSub.textContent = `Loading model: ${progress}%`;
            }

            const prog = Math.floor(progress / 25) * 25;
            const key = `${modelId}:${data.file}`;

            if (prog > 0 && lastProgress[key] !== prog) {
                lastProgress[key] = prog;
                appendLog("transformersLogs", `Loading ${data.file}: ${prog}%`, "info");
            }
        } else if (data.status === 'ready' || data.status === 'done') {
            appendLog("transformersLogs", `Loaded ${data.file || modelId}.`, "info");
        }
    } else if (status === 'model_ready') {
        const elapsed = modelLoadStartedAt > 0
            ? ((performance.now() - modelLoadStartedAt) / 1000).toFixed(2)
            : "0.00";

        appendLog("transformersLogs", `Model ready: ${modelId} (${elapsed}s).`, "info");
    } else if (status === 'all_ready') {
        isModelLoaded = true;
        tfjsDot.classList.add("active");
        voiceTitle.textContent = "Microphone Muted";
        voiceSub.textContent = "Hold the orb to speak";

        appendLog("transformersLogs", "Required AI models are ready.", "info");
        appendLog("transformersLogs", "Browser cache is enabled — refreshes should reuse downloaded model files.", "info");
    } else if (status === 'translated') {
        handleTranslationResult(result);
    } else if (status === 'error') {
        isModelLoaded = false;
        tfjsDot.classList.remove("active");
        appendLog("transformersLogs", `Error: ${error}`, "error");
        voiceTitle.textContent = "Model Error";
        voiceSub.textContent = "Check the AI model logs";
    }
};

function triggerModelLoad() {
    isModelLoaded = false;
    tfjsDot.classList.remove("active");
    lastProgress = {};

    voiceTitle.textContent = "Loading Models...";
    voiceSub.textContent = "First visit downloads once; later visits use the browser cache.";

    appendLog("transformersLogs", `Requested: ${sourceLangSelect.value} → ${targetLangSelect.value}`, "info");

    tfWorker.postMessage({
        action: 'load',
        src_lang: opusMap[sourceLangSelect.value],
        tgt_lang: opusMap[targetLangSelect.value]
    });
}

window.addEventListener('DOMContentLoaded', triggerModelLoad);

// -------------------------------------------------------------------
// TRANSLATION UI HANDLERS
// -------------------------------------------------------------------
function handleTranscriptionResult(transcript) {
    appendLog("sttLogs", `Speech Detected: "${transcript}"`, "info");
    if (emptyMessage) emptyMessage.style.display = "none";

    activeCard = document.createElement("div");
    activeCard.className = "chat-card";
    activeCard.dataset.startTime = performance.now();

    const meta = document.createElement("div");
    meta.className = "meta-tag";
    meta.innerHTML = `<span>●</span> ${sourceLangSelect.options[sourceLangSelect.selectedIndex].text}`;

    const transcriptElem = document.createElement("div");
    transcriptElem.className = "transcript-bubble";
    transcriptElem.textContent = transcript;

    const targetMeta = document.createElement("div");
    targetMeta.className = "meta-tag";
    targetMeta.style.color = "#d8b4fe";
    targetMeta.style.marginTop = "8px";
    targetMeta.style.paddingTop = "8px";
    targetMeta.style.borderTop = "1px solid rgba(255, 255, 255, 0.06)";
    targetMeta.innerHTML = `<span>●</span> ${targetLangSelect.options[targetLangSelect.selectedIndex].text}`;

    const translationElem = document.createElement("div");
    translationElem.className = "translation-bubble";
    translationElem.style.borderTop = "none";
    translationElem.style.paddingTop = "4px";
    translationElem.innerHTML = `
        <div class="translating-wrapper">
            <div class="glowing-orb">
                <div class="pulse-ring"></div>
                <div class="core"></div>
            </div>
            <span class="translating-text">Translating</span>
        </div>
    `;

    activeCard.appendChild(meta);
    activeCard.appendChild(transcriptElem);
    activeCard.appendChild(targetMeta);
    activeCard.appendChild(translationElem);

    chatContainer.appendChild(activeCard);
    chatContainer.scrollTop = chatContainer.scrollHeight;

    appendLog("transformersLogs", "Executing local client-side translation...", "info");

    tfWorker.postMessage({
        action: 'translate',
        text: transcript,
        src_lang: opusMap[sourceLangSelect.value],
        tgt_lang: opusMap[targetLangSelect.value]
    });
}

function handleTranslationResult(translation) {
    if (!activeCard || activeCard.dataset.completed === "true") return;
    activeCard.dataset.completed = "true";

    const translationEndTime = performance.now();
    const startTime = parseFloat(activeCard.dataset.startTime);
    const translationLatency = !isNaN(startTime)
        ? ((translationEndTime - startTime) / 1000).toFixed(2)
        : "0.00";

    appendLog("transformersLogs", `Output: "${translation}"`, "info");
    appendLog("transformersLogs", `Translation completed in ${translationLatency}s.`, "info");
    appendLog("ttsLogs", `Synthesizing audio...`, "info");

    const translationElem = activeCard.querySelector(".translation-bubble");
    if (translationElem) {
        translationElem.style.opacity = '0';
        translationElem.textContent = translation;

        setTimeout(() => {
            translationElem.style.opacity = '1';
            chatContainer.scrollTop = chatContainer.scrollHeight;
        }, 50);
    }

    speakText(translation, targetLangSelect.value);
}

// -------------------------------------------------------------------
// TEXT-TO-SPEECH
// -------------------------------------------------------------------
window.currentUtterance = null;
function speakText(text, langCode) {
    if (!('speechSynthesis' in window)) return;
    window.speechSynthesis.cancel();

    const utterance = new SpeechSynthesisUtterance(text);
    window.currentUtterance = utterance;

    const bcp47Map = { "es": "es-ES", "en": "en-US", "hi": "hi-IN", "ja": "ja-JP", "zh": "zh-CN" };
    utterance.lang = bcp47Map[langCode] || langCode;
    utterance.rate = 1.0;

    const voices = window.speechSynthesis.getVoices();
    const selectedVoice = voices.find(v => v.lang.toLowerCase().startsWith(langCode.toLowerCase()));
    if (selectedVoice) utterance.voice = selectedVoice;

    const robot = document.getElementById('aiRobot');

    utterance.onstart = () => { if (robot) robot.classList.add('speaking'); };
    utterance.onend = () => {
        if (robot) robot.classList.remove('speaking');
        appendLog("ttsLogs", "Playback complete.", "info");
        window.currentUtterance = null;
    };
    utterance.onerror = () => {
        if (robot) robot.classList.remove('speaking');
        window.currentUtterance = null;
    };

    window.speechSynthesis.speak(utterance);
}

if ('speechSynthesis' in window) {
    window.speechSynthesis.onvoiceschanged = () => window.speechSynthesis.getVoices();
}

// -------------------------------------------------------------------
// SPEECH-TO-TEXT (Push To Talk)
// -------------------------------------------------------------------
const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
let recognition = null;

function setupSpeechRecognition() {
    if (!SpeechRecognition) {
        alert("Web Speech API is not supported in this browser.");
        return;
    }

    recognition = new SpeechRecognition();
    recognition.continuous = false;
    recognition.interimResults = false;

    const bcp47Map = { "hi": "hi-IN", "en": "en-US", "es": "es-ES", "ja": "ja-JP", "zh": "zh-CN" };
    recognition.lang = bcp47Map[sourceLangSelect.value] || sourceLangSelect.value;

    recognition.onresult = (event) => {
        const lastResultIndex = event.results.length - 1;
        const transcript = event.results[lastResultIndex][0].transcript.trim();
        if (transcript) handleTranscriptionResult(transcript);
    };

    recognition.onerror = (event) => {
        console.error("Speech recognition error:", event.error);
        if (event.error === 'not-allowed') {
            stopCapture();
            voiceTitle.textContent = "Mic Blocked";
            voiceSub.textContent = "Check browser permissions";
        }
    };
}

function handleLanguageChange() {
    const bcp47Map = { "hi": "hi-IN", "en": "en-US", "es": "es-ES", "ja": "ja-JP", "zh": "zh-CN" };
    if (recognition) {
        recognition.lang = bcp47Map[sourceLangSelect.value] || sourceLangSelect.value;
    }
    triggerModelLoad();
}

sourceLangSelect.addEventListener("change", handleLanguageChange);
targetLangSelect.addEventListener("change", handleLanguageChange);

// -------------------------------------------------------------------
// PUSH-TO-TALK MECHANIC
// -------------------------------------------------------------------
function startCapture() {
    if (!isModelLoaded) {
        voiceSub.textContent = "Please wait, models are loading...";
        return;
    }
    if (isHolding) return;
    isHolding = true;

    if (!recognition) setupSpeechRecognition();

    micOrb.classList.remove("muted");
    micOrb.classList.add("active");
    waveform.classList.add("listening");
    voiceTitle.textContent = "Listening Active";
    voiceSub.textContent = "Release to translate...";
    micOrb.innerHTML = `<svg class="mic-icon" viewBox="0 0 24 24"><path d="M12 14c1.66 0 3-1.34 3-3V5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3z"/><path d="M17 11c0 2.76-2.24 5-5 5s-5-2.24-5-5H5c0 3.53 2.61 6.43 6 6.92V21h2v-3.08c3.39-.49 6-3.39 6-6.92h-2z"/></svg>`;

    try { recognition.start(); } catch (e) { }
}

function stopCapture() {
    if (!isHolding) return;
    isHolding = false;

    micOrb.classList.add("muted");
    micOrb.classList.remove("active");
    waveform.classList.remove("listening");
    voiceTitle.textContent = "Microphone Muted";
    voiceSub.textContent = "Hold the orb to speak";
    micOrb.innerHTML = `<svg class="mic-icon" viewBox="0 0 24 24"><path d="M19 11h-1.7c0 .74-.16 1.43-.43 2.05l1.23 1.23c.56-.98.9-2.09.9-3.28zm-4.02 3.28l-1.5 1.5C12.95 15.93 12.49 16 12 16c-2.21 0-4-1.79-4-4H6.3c0 2.89 2.11 5.3 4.85 5.82V21h1.7v-3.18c.84-.16 1.62-.5 2.29-.97zM2.1 4.1L.69 5.51l2.43 2.43C3.04 8.79 3 9.38 3 10v1c0 2.76 2.24 5 5 5h3.09l4.4 4.4 1.41-1.41L2.1 4.1zM12 5c1.66 0 3 1.34 3 3v2.17l-6 6V8c0-1.66 1.34-3 3-3z"/></svg>`;

    try { recognition.stop(); } catch (e) { }
}

micOrb.addEventListener('mousedown', startCapture);
micOrb.addEventListener('touchstart', (e) => {
    e.preventDefault();
    startCapture();
});

window.addEventListener('mouseup', stopCapture);
window.addEventListener('touchend', stopCapture);

const clearButton = document.getElementById("clearButton");
clearButton.addEventListener("click", () => {
    chatContainer.innerHTML = '';
    if (emptyMessage) {
        chatContainer.appendChild(emptyMessage);
        emptyMessage.style.display = "block";
    }
    activeCard = null;
    appendLog("transformersLogs", "Chat history cleared from UI.", "info");
});