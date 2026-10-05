using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.Extensions.AI;
using System.Text.RegularExpressions;

namespace NeuralWorld.Space.LiveSpeech.Translator.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly IChatClient _chatClient;

        public TranslationService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        //it is for Ollama Qwen-2.5 3B model
        //    public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default)
        //    {
        //        if (string.IsNullOrWhiteSpace(text))
        //        {
        //            return string.Empty;
        //        }

        //        var languages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        //        {
        //            ["en"] = "English",
        //            ["hi"] = "Hindi",
        //            ["es"] = "Spanish",
        //            ["ja"] = "Japanese",
        //            ["ko"] = "Korean"
        //        };

        //        string srcName = languages.TryGetValue(sourceLanguage, out var s) ? s : sourceLanguage;
        //        string tgtName = languages.TryGetValue(targetLanguage, out var t) ? t : targetLanguage;

        //        // Format the prompt as a strict dataset completion to bypass the chat persona
        //        var chatMessages = new List<ChatMessage>
        //{
        //    new ChatMessage(ChatRole.System, "You are a raw translation engine. Never use parentheses. Never translate to English unless requested."),
        //    new ChatMessage(ChatRole.User, $"{srcName}: {text}\n{tgtName}:")
        //};

        //        var response = await _chatClient.GetResponseAsync(chatMessages,
        //            new ChatOptions
        //            {
        //                Temperature = 0.0f,
        //                MaxOutputTokens = 64
        //            },
        //            cancellationToken);

        //        var translation = response.Text ?? string.Empty;

        //        // 1. Remove the dictionary parentheses the model previously hallucinated
        //        translation = translation.Replace("(", string.Empty).Replace(")", string.Empty);

        //        // 2. Keep only the first line to prevent the model from hallucinating a new "Hindi: " line
        //        translation = translation.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

        //        // 3. Clean up any lingering prefixes
        //        var prefixes = new[] { $"{tgtName}:", "Translation:", "Output:" };
        //        foreach (var prefix in prefixes)
        //        {
        //            if (translation.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        //            {
        //                translation = translation.Substring(prefix.Length);
        //            }
        //        }

        //        return translation.Replace("\"", string.Empty).Trim();
        //    }

        //the below code is for Qwen-3 8B model
        public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var prompt = $"""
    Translate the following text from {sourceLanguage} to {targetLanguage}.

    Rules:
    - Return only the translated text.
    - Do not explain anything.
    - Do not add quotes.
    - Do not add labels such as "Translation:".
    - Preserve the meaning and tone of the original text.
    - Do not answer the content as a question; only translate it.

    Text:
    {text}
    """;

            var response = await _chatClient.GetResponseAsync(prompt,
                new ChatOptions
                {
                    Temperature = 0.0f,
                    MaxOutputTokens = 512,
                    AdditionalProperties = new()
                    {
                        // Natively disables Ollama's reasoning phase to prevent <think> generation entirely
                        ["think"] = false
                    }
                },
                cancellationToken);

            var translation = response.Text ?? string.Empty;

            // Keep your existing cleanup rules (Regex for <think> is no longer needed)
            translation = translation.Replace("\\n", " ");
            translation = System.Text.RegularExpressions.Regex.Replace(translation, @"^\s*\.\s*", string.Empty);
            translation = System.Text.RegularExpressions.Regex.Replace(translation, @"\s+", " ").Trim();

            return translation;
        }
        //public async Task<string> TranslateAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default)
        //{
        //    if (string.IsNullOrWhiteSpace(text))
        //    {
        //        return string.Empty;
        //    }

        //    var prompt = $"""
        //                    Translate the following text from {sourceLanguage} to {targetLanguage}.

        //                    Rules:
        //                    - Return only the translated text.
        //                    - Do not explain anything.
        //                    - Do not add quotes.
        //                    - Do not add labels such as "Translation:".
        //                    - Preserve the meaning and tone of the original text.
        //                    - Do not answer the content as a question; only translate it.

        //                    Text:
        //                    {text}
        //                    """;

        //    var response = await _chatClient.GetResponseAsync(prompt,
        //    new ChatOptions
        //    {
        //        Temperature = 0.2f,
        //        MaxOutputTokens = 128
        //    },
        //    cancellationToken);

        //    var translation = response.Text;

        //    translation = Regex.Replace(
        //        translation,
        //        @"<think>.*?</think>",
        //        string.Empty,
        //        RegexOptions.Singleline | RegexOptions.IgnoreCase);

        //    translation = translation
        //        .Replace("\\n", " ");

        //    translation = Regex.Replace(translation, @"^\s*\.\s*", string.Empty);
        //    translation = Regex.Replace(translation, @"\s+", " ").Trim();

        //    return translation;
        //    // return response.Text.Trim();
        //}
    }
}
