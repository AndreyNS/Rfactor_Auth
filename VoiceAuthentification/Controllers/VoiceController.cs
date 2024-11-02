using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoiceAuthentification.Models;

namespace VoiceAuthentification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private string infoMessage = string.Empty;

        private readonly ILogger<VoiceController> _logger;
        
        public VoiceController(ILogger<VoiceController> logger)
        {
            _logger = logger;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyVoice([FromBody] VoiceData voiceData)
        {
            if (!(Request.Body.Length > 0))
            {
                infoMessage = "Audio stream empty";
                _logger.LogError(infoMessage);
                return BadRequest(infoMessage);
            }

            await GetVoiceStream(Request.Body);
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterVoice()
        {
            infoMessage = "The voice has been successfully analyzed for registration, a refund is being made";
            if (!(Request.Body.Length > 0))
            {
                infoMessage = "Audio stream empty";
                _logger.LogError(infoMessage);
                return BadRequest(infoMessage);
            }

            await GetVoiceStream(Request.Body);

            _logger.LogInformation(infoMessage);
            return Ok();
        }

        private async Task GetVoiceStream(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();
            }
        }
    }
}
