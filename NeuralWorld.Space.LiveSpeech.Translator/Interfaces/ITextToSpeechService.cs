namespace NeuralWorld.Space.LiveSpeech.Translator.Interfaces
{
    public interface ITextToSpeechService
    {
        Task<byte[]> SynthesizeAsync(string text, string language, CancellationToken cancellationToken = default);
    }
}
