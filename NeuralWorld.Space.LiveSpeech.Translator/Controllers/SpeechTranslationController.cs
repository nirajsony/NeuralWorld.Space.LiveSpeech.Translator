using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeuralWorld.Space.LiveSpeech.Translator.Controllers
{
    [Route("api/speech-translation")]
    [ApiController]
    public class SpeechTranslationController : ControllerBase
    {
        private readonly ISpeechSessionService _speechSessionService;

        public SpeechTranslationController(ISpeechSessionService speechSessionService)
        {
            _speechSessionService = speechSessionService;
        }

        [HttpGet("test-hindi-to-spanish")]
        public async Task<IActionResult> TestHindiToSpanish(CancellationToken cancellationToken)
        {
            var audioFile = @"C:\Users\Niraj\Downloads\AI_MODEL\hindi-test.wav";

            var audio = await _speechSessionService.TranslateSpeechAsync(audioFile, "hi", "es", cancellationToken);

            return File(audio, "audio/wav", "hindi-to-spanish.wav");
        }
    }
}
