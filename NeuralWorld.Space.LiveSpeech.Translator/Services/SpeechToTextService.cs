using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NeuralWorld.Space.LiveSpeech.Translator.Services
{
    public class SpeechToTextService : ISpeechToTextService
    {
        private readonly HttpClient _httpClient;

        public SpeechToTextService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("http://127.0.0.1:8080"); //whisper server
        }

        // New approach for byte array input
        public async Task<string> TranscribeAsync(byte[] audioData, string language, CancellationToken cancellationToken = default)
        {
            if (audioData == null || audioData.Length == 0)
            {
                throw new ArgumentException("Audio data cannot be empty.", nameof(audioData));
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException("Language cannot be empty.", nameof(language));
            }

            using var content = new MultipartFormDataContent();

            // Wrap the RAM bytes directly into an HTTP content object
            using var byteContent = new ByteArrayContent(audioData);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            // The Whisper server requires the form field to be named "file".
            // Passing a fake filename ("audio.wav") satisfies the endpoint's multipart parser.
            content.Add(byteContent, "file", "audio.wav");
            content.Add(new StringContent(language), "language");
            content.Add(new StringContent("json"), "response_format");

            using var response = await _httpClient.PostAsync("/inference", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Whisper server returned {(int)response.StatusCode} " +
                    $"({response.StatusCode}). Response: {error}");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<WhisperResponse>(responseStream, cancellationToken: cancellationToken);

            return result?.Text?.Trim() ?? string.Empty;
        }


        //old approach for file input
        public async Task<string> TranscribeAsync(string audioFile, string language, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(audioFile))
            {
                throw new ArgumentException("Audio file path cannot be empty.", nameof(audioFile));
            }

            if (!File.Exists(audioFile))
            {
                throw new FileNotFoundException(
                    "Audio file was not found.",
                    audioFile);
            }

            if (string.IsNullOrWhiteSpace(language))
            {
                throw new ArgumentException(
                    "Language cannot be empty.",
                    nameof(language));
            }

            await using var fileStream = File.OpenRead(audioFile);

            using var content = new MultipartFormDataContent();

            using var fileContent = new StreamContent(fileStream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            content.Add(fileContent, "file", Path.GetFileName(audioFile));

            content.Add(new StringContent(language), "language");

            content.Add(new StringContent("json"), "response_format");

            using var response = await _httpClient.PostAsync("/inference", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);

                throw new InvalidOperationException(
                    $"Whisper server returned {(int)response.StatusCode} " +
                    $"({response.StatusCode}). Response: {error}");
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

            var result = await JsonSerializer.DeserializeAsync<WhisperResponse>(responseStream, cancellationToken: cancellationToken);

            return result?.Text?.Trim() ?? string.Empty;
        }

        private sealed class WhisperResponse
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
        //private const string WhisperExecutable = @"C:\Users\Niraj\Downloads\AI_MODEL\whisper\whisper-cli.exe";

        //private const string WhisperModel = @"C:\Users\Niraj\Downloads\AI_MODEL\ggml-large-v3-turbo-q5_0.bin";

        //public async Task<string> TranscribeAsync(string audioFile, string language, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(audioFile))
        //    {
        //        throw new ArgumentException(
        //            "Audio file path cannot be empty.",
        //            nameof(audioFile));
        //    }

        //    if (!File.Exists(audioFile))
        //    {
        //        throw new FileNotFoundException(
        //            "Audio file was not found.",
        //            audioFile);
        //    }

        //    if (!File.Exists(WhisperExecutable))
        //    {
        //        throw new FileNotFoundException(
        //            "Whisper executable was not found.",
        //            WhisperExecutable);
        //    }

        //    if (!File.Exists(WhisperModel))
        //    {
        //        throw new FileNotFoundException(
        //            "Whisper model was not found.",
        //            WhisperModel);
        //    }

        //    if (string.IsNullOrWhiteSpace(language))
        //    {
        //        throw new ArgumentException(
        //            "Language cannot be empty.",
        //            nameof(language));
        //    }

        //    var processStartInfo = new ProcessStartInfo
        //    {
        //        FileName = WhisperExecutable,
        //        RedirectStandardOutput = true,
        //        RedirectStandardError = true,
        //        StandardOutputEncoding = Encoding.UTF8,
        //        StandardErrorEncoding = Encoding.UTF8,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    processStartInfo.ArgumentList.Add("-m");
        //    processStartInfo.ArgumentList.Add(WhisperModel);

        //    processStartInfo.ArgumentList.Add("-f");
        //    processStartInfo.ArgumentList.Add(audioFile);

        //    processStartInfo.ArgumentList.Add("-l");
        //    processStartInfo.ArgumentList.Add(language);

        //    processStartInfo.ArgumentList.Add("-nt");

        //    using var process = new Process
        //    {
        //        StartInfo = processStartInfo
        //    };

        //    process.Start();

        //    var outputTask = process.StandardOutput.ReadToEndAsync(
        //        cancellationToken);

        //    var errorTask = process.StandardError.ReadToEndAsync(
        //        cancellationToken);

        //    await process.WaitForExitAsync(cancellationToken);

        //    var output = await outputTask;
        //    var error = await errorTask;

        //    if (process.ExitCode != 0)
        //    {
        //        throw new InvalidOperationException(
        //            $"Whisper failed with exit code {process.ExitCode}. " +
        //            $"Error: {error}");
        //    }

        //    return output.Trim();
        //}
    }
}
