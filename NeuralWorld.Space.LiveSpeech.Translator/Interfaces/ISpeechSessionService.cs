namespace NeuralWorld.Space.LiveSpeech.Translator.Interfaces
{
    public interface ISpeechSessionService
    {
        Task<byte[]> TranslateSpeechAsync(
            string audioFile,
            string sourceLanguage,
            string targetLanguage,
            CancellationToken cancellationToken = default);
    }
}
