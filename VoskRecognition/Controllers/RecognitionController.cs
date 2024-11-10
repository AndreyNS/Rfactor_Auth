using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using VoskRecognition.Interfaces;
using VoskRecognition.Services;

namespace VoskRecognition.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecognitionController : ControllerBase
    {
        private readonly ILogger<RecognitionManager> _logger;
        private readonly IRecogtion _manager;

        public RecognitionController(ILogger<RecognitionManager> logger, IRecogtion manager)
        {
            _logger = logger;
            _manager = manager;
        }

        [HttpPost]
        public async Task<IActionResult> VoiceRecognition(IFormFile voice)
        {
            MemoryStream ms = new MemoryStream();
            try
            {
                await voice.CopyToAsync(ms);
                ms.Position = 0;

                string phrase = await _manager.RecognizeSpeech(ms);
                ms.Close();

                if (string.IsNullOrWhiteSpace(phrase))
                {
                    _logger.LogWarning($"[{nameof(VoiceRecognition)}] Фраза пуста.");
                    return NoContent();
                }

                phrase = Convert.ToString(JsonConvert.DeserializeObject<dynamic>(phrase)?.text);

                _logger.LogInformation($"[{nameof(VoiceRecognition)}] Фраза успешно возвращена.");
                return Ok(phrase);
            }
            catch (Exception ex)
            {
                ms.Close();
                _logger.LogError(ex, $"[{nameof(VoiceRecognition)}] Ошибка распознования.");

                return StatusCode(StatusCodes.Status500InternalServerError);
            }         
        }
    }
}
