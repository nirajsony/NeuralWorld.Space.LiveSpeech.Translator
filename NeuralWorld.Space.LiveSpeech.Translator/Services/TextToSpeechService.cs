using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace NeuralWorld.Space.LiveSpeech.Translator.Services
{
    public class TextToSpeechService : ITextToSpeechService
    {
        private const string PiperBaseUrl = "http://localhost:5000";

        private readonly HttpClient _httpClient;

        private static readonly Dictionary<string, string> VoiceMap =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["en"] = "en_US-lessac-medium",
                ["hi"] = "hi_IN-pratham-medium",
                ["es"] = "es_ES-sharvard-medium",
                ["ja"] = "ja_JP-hi_fi_captain-medium",
                ["ko"] = "ko_KR-kss-medium"
            };

        public TextToSpeechService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> SynthesizeAsync(string text, string language, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException(
                    "Text cannot be empty.",
                    nameof(text));
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException(
                    "Language cannot be empty.",
                    nameof(language));
            }

            var languageCode = language.Split('-', '_')[0].ToLowerInvariant();

            if (!VoiceMap.TryGetValue(languageCode, out var voice))
            {
                throw new NotSupportedException(
                    $"No Piper voice is configured for language '{language}'.");
            }

            var requestBody = new
            {
                text,
                voice
            };

            var json = JsonSerializer.Serialize(requestBody);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync($"{PiperBaseUrl}/synthesize", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    $"Piper returned {(int)response.StatusCode} " +
                    $"({response.StatusCode}). Response: {error}");
            }

            var audio = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (audio.Length == 0)
            {
                throw new InvalidOperationException(
                    "Piper returned an empty audio response.");
            }

            return audio;
        }
        //private const string TtsRoot = @"C:\Users\Niraj\Downloads\AI_MODEL\TTS_MODELS";

        //public async Task<byte[]> SynthesizeAsync(string text, string language, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //    {
        //        throw new ArgumentException(
        //            "Text cannot be empty.",
        //            nameof(text));
        //    }

        //    if (string.IsNullOrWhiteSpace(language))
        //    {
        //        throw new ArgumentException(
        //            "Language cannot be empty.",
        //            nameof(language));
        //    }

        //    var voice = GetVoice(language);

        //    if (voice is null)
        //    {
        //        throw new NotSupportedException(
        //            $"TTS is not configured for language '{language}'.");
        //    }

        //    var voiceDirectory = Path.Combine(TtsRoot, voice.DirectoryName);

        //    if (!Directory.Exists(voiceDirectory))
        //    {
        //        throw new DirectoryNotFoundException(
        //            $"Piper voice directory was not found: {voiceDirectory}");
        //    }

        //    var outputFile = Path.Combine(Path.GetTempPath(), $"tts-{Guid.NewGuid():N}.wav");

        //    try
        //    {
        //        var processStartInfo = new ProcessStartInfo
        //        {
        //            FileName = "py",
        //            RedirectStandardOutput = true,
        //            RedirectStandardError = true,
        //            RedirectStandardInput = true,
        //            UseShellExecute = false,
        //            CreateNoWindow = true
        //        };

        //        processStartInfo.ArgumentList.Add("-m");
        //        processStartInfo.ArgumentList.Add("piper");

        //        processStartInfo.ArgumentList.Add("-m");
        //        processStartInfo.ArgumentList.Add(voice.ModelName);

        //        processStartInfo.ArgumentList.Add("--data-dir");
        //        processStartInfo.ArgumentList.Add(voiceDirectory);

        //        processStartInfo.ArgumentList.Add("-f");
        //        processStartInfo.ArgumentList.Add(outputFile);

        //        using var process = new Process
        //        {
        //            StartInfo = processStartInfo
        //        };

        //        process.Start();

        //        await process.StandardInput.WriteAsync(text);
        //        process.StandardInput.Close();

        //        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        //        await process.WaitForExitAsync(cancellationToken);

        //        var error = await errorTask;

        //        if (process.ExitCode != 0)
        //        {
        //            throw new InvalidOperationException(
        //                $"Piper failed with exit code {process.ExitCode}. " +
        //                $"Error: {error}");
        //        }

        //        if (!File.Exists(outputFile))
        //        {
        //            throw new InvalidOperationException(
        //                "Piper completed successfully, but no WAV file was generated.");
        //        }

        //        return await File.ReadAllBytesAsync(outputFile, cancellationToken);
        //    }
        //    finally
        //    {
        //        if (File.Exists(outputFile))
        //        {
        //            File.Delete(outputFile);
        //        }
        //    }
        //}

        //private static PiperVoice? GetVoice(string language)
        //{
        //    var languageCode = language.Split('-', '_')[0].ToLowerInvariant();

        //    return languageCode switch
        //    {
        //        "en" => new PiperVoice(
        //            "piper_en_US",
        //            "en_US-lessac-medium"),

        //        "hi" => new PiperVoice(
        //            "piper_hi_IN",
        //            "hi_IN-pratham-medium"),

        //        "es" => new PiperVoice(
        //            "piper_es_ES",
        //            "es_ES-sharvard-medium"),

        //        "ja" => new PiperVoice(
        //            "piper_ja_JP",
        //            "ja_JP-hi_fi_captain-medium"),

        //        _ => null
        //    };
        //}

        //private sealed record PiperVoice(string DirectoryName, string ModelName);
    }
}
