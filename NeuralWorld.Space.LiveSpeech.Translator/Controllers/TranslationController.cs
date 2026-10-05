using NeuralWorld.Space.LiveSpeech.Translator.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NeuralWorld.Space.LiveSpeech.Translator.Controllers
{
    [Route("api/translation")]
    [ApiController]
    public class TranslationController : ControllerBase
    {
        private readonly ITranslationService _translationService;

        public TranslationController(ITranslationService translationService)
        {
            _translationService = translationService;
        }

        [HttpPost]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request, CancellationToken cancellationToken)
        {
            var translation = await _translationService.TranslateAsync(
                request.Text,
                request.SourceLanguage,
                request.TargetLanguage,
                cancellationToken);

            return Ok(new
            {
                sourceLanguage = request.SourceLanguage,
                targetLanguage = request.TargetLanguage,
                originalText = request.Text,
                translation
            });
        }
        public sealed record TranslationRequest(
    string Text,
    string SourceLanguage,
    string TargetLanguage);
    }
}
