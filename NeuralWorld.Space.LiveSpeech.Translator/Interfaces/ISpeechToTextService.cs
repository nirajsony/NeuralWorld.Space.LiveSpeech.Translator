namespace NeuralWorld.Space.LiveSpeech.Translator.Interfaces
{
    public interface ISpeechToTextService
    {
        //old approach
        Task<string> TranscribeAsync(string audioFile, string language, CancellationToken cancellationToken = default);

        //new approach for byte array input
        Task<string> TranscribeAsync(byte[] audioData, string language, CancellationToken cancellationToken = default);
        
    
}
}
