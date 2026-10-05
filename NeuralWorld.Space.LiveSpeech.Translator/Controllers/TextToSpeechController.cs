using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeuralWorld.Space.LiveSpeech.Translator.Controllers
{
    [Route("api/text-to-speech")]
    [ApiController]
    public class TextToSpeechController : ControllerBase
    {
        private readonly ITextToSpeechService _textToSpeechService;

        public TextToSpeechController(ITextToSpeechService textToSpeechService)
        {
            _textToSpeechService = textToSpeechService;
        }

        [HttpGet("test-spanish")]
        public async Task<IActionResult> TestSpanish(CancellationToken cancellationToken)
        {
            var audio = await _textToSpeechService.SynthesizeAsync("Hola, ¿cómo estás?", "es", cancellationToken);

            return File(audio, "audio/wav", "spanish-from-dotnet.wav");
        }
    }
}
