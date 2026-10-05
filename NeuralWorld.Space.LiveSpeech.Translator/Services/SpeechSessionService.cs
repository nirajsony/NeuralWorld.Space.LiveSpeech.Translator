using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;

namespace NeuralWorld.Space.LiveSpeech.Translator.Services
{
    public class SpeechSessionService : ISpeechSessionService
    {
        private readonly ISpeechToTextService _speechToTextService;
        private readonly ITranslationService _translationService;
        private readonly ITextToSpeechService _textToSpeechService;

        public SpeechSessionService(ISpeechToTextService speechToTextService, ITranslationService translationService, ITextToSpeechService textToSpeechService)
        {
            _speechToTextService = speechToTextService;
            _translationService = translationService;
            _textToSpeechService = textToSpeechService;
        }

        public async Task<byte[]> TranslateSpeechAsync(string audioFile, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken = default)
        {
            // Step 1: Speech → Text
            var transcript = await _speechToTextService.TranscribeAsync(audioFile, sourceLanguage, cancellationToken);

            if (string.IsNullOrWhiteSpace(transcript))
            {
                throw new InvalidOperationException(
                    "No speech could be transcribed from the audio.");
            }

            // Step 2: Text → Translated Text
            var translatedText = await _translationService.TranslateAsync(transcript, sourceLanguage, targetLanguage, cancellationToken);

            if (string.IsNullOrWhiteSpace(translatedText))
            {
                throw new InvalidOperationException(
                    "Translation returned empty text.");
            }

            // Step 3: Translated Text → Speech
            var audio = await _textToSpeechService.SynthesizeAsync(translatedText, targetLanguage, cancellationToken);

            if (audio.Length == 0)
            {
                throw new InvalidOperationException("Text-to-speech returned empty audio.");
            }

            return audio;
        }
    }
}
