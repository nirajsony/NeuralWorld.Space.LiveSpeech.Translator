using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace NeuralWorld.Space.LiveSpeech.Translator.Hubs
{
    public class TranslationHub : Hub
    {
        private readonly ITranslationService _translationService;

        public TranslationHub(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        public string Ping() => "SignalR connection is working.";

        public async Task ReceiveTranscript(string transcript, string sourceLang = "hi", string targetLang = "es")
        {
            if (string.IsNullOrWhiteSpace(transcript)) return;

            await Clients.Caller.SendAsync("WhisperLogReceived", $"[STT - {sourceLang}]: {transcript}");
            await Clients.Caller.SendAsync("TranscriptReceived", transcript);
            await Clients.Caller.SendAsync("OllamaLogReceived", $"Generating translation to {targetLang}...");

            var translation = await _translationService.TranslateAsync(transcript, sourceLang, targetLang);

            await Clients.Caller.SendAsync("OllamaLogReceived", $"[Translation - {targetLang}]: {translation}");
            await Clients.Caller.SendAsync("TranslationReceived", translation);
            await Clients.Caller.SendAsync("PiperLogReceived", "Translation ready. Browser synthesizing audio...");
        }

        // New approach: Receive speech segment as byte array and process in memory
        //public async Task ReceiveSpeechSegment(byte[] audioData, string sourceLang = "hi", string targetLang = "es")
        //{
        //    const int maxAudioSize = 1024 * 1024; // 1 MB limit

        //    if (audioData is null || audioData.Length == 0 || audioData.Length > maxAudioSize)
        //    {
        //        return;
        //    }

        //    string receivedMsg = $"Received speech segment: {audioData.Length} bytes | {sourceLang} -> {targetLang}";
        //    Console.WriteLine(receivedMsg);
        //    await Clients.Caller.SendAsync("WhisperLogReceived", receivedMsg);
        //    await Clients.Caller.SendAsync("WhisperLogReceived", "Processing audio with Whisper model...");

        //    // 1. Build the WAV format bytes entirely in memory
        //    var wavSegment = EnsureWavHeader(audioData);

        //    // 2. Transcribe audio straight from RAM (no File.WriteAllBytesAsync)
        //    var transcript = await _speechToTextService.TranscribeAsync(wavSegment, sourceLang);

        //    if (string.IsNullOrWhiteSpace(transcript))
        //    {
        //        await Clients.Caller.SendAsync("WhisperLogReceived", "No speech detected in segment.");
        //        return;
        //    }

        //    string transcriptMsg = $"[Whisper STT - {sourceLang}]: {transcript}";
        //    Console.WriteLine(transcriptMsg);
        //    await Clients.Caller.SendAsync("WhisperLogReceived", transcriptMsg);

        //    await Clients.Caller.SendAsync("OllamaLogReceived", $"Generating translation to {targetLang}...");

        //    // 3. Translate text
        //    var translation = await _translationService.TranslateAsync(transcript, sourceLang, targetLang);

        //    string translationMsg = $"[Ollama LLM - {targetLang}]: {translation}";
        //    Console.WriteLine(translationMsg);
        //    await Clients.Caller.SendAsync("OllamaLogReceived", translationMsg);

        //    // 4. Emit results back to the browser
        //    await Clients.Caller.SendAsync("TranscriptReceived", transcript);
        //    await Clients.Caller.SendAsync("TranslationReceived", translation);
        //    await Clients.Caller.SendAsync("PiperLogReceived", "Translation ready. Dispatching text for TTS generation...");
        //}


        //---OLD APPROACH: Using temporary WAV file on disk (commented out)---
        // Single method signature without overloads
        //public async Task ReceiveSpeechSegment(byte[] audioData, string sourceLang = "hi", string targetLang = "es")
        //{
        //    const int maxAudioSize = 1024 * 1024; // 1 MB limit

        //    if (audioData is null || audioData.Length == 0)
        //    {
        //        return;
        //    }

        //    if (audioData.Length > maxAudioSize)
        //    {
        //        await Clients.Caller.SendAsync(
        //            "TranslationError",
        //            "Speech segment is too large.");
        //        return;
        //    }

        //    string receivedMsg = $"Received speech segment: {audioData.Length} bytes | {sourceLang} -> {targetLang}";
        //    Console.WriteLine(receivedMsg);

        //    // Send log to Whisper UI Panel
        //    await Clients.Caller.SendAsync("WhisperLogReceived", receivedMsg);
        //    await Clients.Caller.SendAsync("WhisperLogReceived", "Processing audio with Whisper model...");

        //    var wavSegment = EnsureWavHeader(audioData);
        //    var wavFile = Path.Combine(Path.GetTempPath(), $"speech-{Guid.NewGuid():N}.wav");

        //    try
        //    {
        //        await File.WriteAllBytesAsync(wavFile, wavSegment);

        //        // 1. Transcribe audio with Whisper
        //        var transcript = await _speechToTextService.TranscribeAsync(wavFile, sourceLang);

        //        if (string.IsNullOrWhiteSpace(transcript))
        //        {
        //            await Clients.Caller.SendAsync("WhisperLogReceived", "No speech detected in segment.");
        //            return;
        //        }

        //        string transcriptMsg = $"[Whisper STT - {sourceLang}]: {transcript}";
        //        Console.WriteLine(transcriptMsg);
        //        await Clients.Caller.SendAsync("WhisperLogReceived", transcriptMsg);

        //        // Send log to Ollama UI Panel
        //        await Clients.Caller.SendAsync("OllamaLogReceived", $"Generating translation to {targetLang}...");

        //        // 2. Translate text with Ollama Qwen
        //        var translation = await _translationService.TranslateAsync(transcript, sourceLang, targetLang);

        //        string translationMsg = $"[Ollama LLM - {targetLang}]: {translation}";
        //        Console.WriteLine(translationMsg);
        //        await Clients.Caller.SendAsync("OllamaLogReceived", translationMsg);

        //        // 3. Emit results to SignalR client
        //        await Clients.Caller.SendAsync("TranscriptReceived", transcript);
        //        await Clients.Caller.SendAsync("TranslationReceived", translation);

        //        // Send log to Piper TTS UI Panel
        //        await Clients.Caller.SendAsync("PiperLogReceived", "Translation ready. Dispatching text for TTS generation...");
        //    }
        //    finally
        //    {
        //        if (File.Exists(wavFile))
        //        {
        //            File.Delete(wavFile);
        //        }
        //    }
        //}

        private static byte[] EnsureWavHeader(byte[] audioData)
        {
            if (audioData.Length >= 12 &&
                audioData[0] == (byte)'R' && audioData[1] == (byte)'I' &&
                audioData[2] == (byte)'F' && audioData[3] == (byte)'F' &&
                audioData[8] == (byte)'W' && audioData[9] == (byte)'A' &&
                audioData[10] == (byte)'V' && audioData[11] == (byte)'E')
            {
                return audioData;
            }

            return CreateWav(audioData);
        }

        private static byte[] CreateWav(byte[] pcmData)
        {
            const int sampleRate = 16000;
            const short channels = 1;
            const short bitsPerSample = 16;

            var byteRate = sampleRate * channels * bitsPerSample / 8;
            var blockAlign = (short)(channels * bitsPerSample / 8);

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            // RIFF header
            writer.Write("RIFF"u8.ToArray());
            writer.Write(36 + pcmData.Length);
            writer.Write("WAVE"u8.ToArray());

            // fmt chunk
            writer.Write("fmt "u8.ToArray());
            writer.Write(16);                // PCM format chunk size
            writer.Write((short)1);          // PCM format
            writer.Write(channels);
            writer.Write(sampleRate);
            writer.Write(byteRate);
            writer.Write(blockAlign);
            writer.Write(bitsPerSample);

            // data chunk
            writer.Write("data"u8.ToArray());
            writer.Write(pcmData.Length);
            writer.Write(pcmData);

            writer.Flush();

            return stream.ToArray();
        }
    }
}