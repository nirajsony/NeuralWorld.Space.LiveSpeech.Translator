using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeuralWorld.Space.LiveSpeech.Translator.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/speech-to-text")]
    [ApiController]
    public class SpeechToTextController : ControllerBase
    {
        private readonly ISpeechToTextService _speechToTextService;

        public SpeechToTextController(ISpeechToTextService speechToTextService)
        {
            _speechToTextService = speechToTextService;
        }

        [HttpGet("test-hindi")]
        public async Task<IActionResult> TestHindi(CancellationToken cancellationToken)
        {
            var audioFile = @"C:\Users\Niraj\Downloads\AI_MODEL\hindi-test.wav";

            var transcript = await _speechToTextService.TranscribeAsync(audioFile, "hi", cancellationToken);

            return Ok(new
            {
                sourceLanguage = "hi",
                transcript
            });
        }
    }
}
