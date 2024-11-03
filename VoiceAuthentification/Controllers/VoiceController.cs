using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using VoiceAuthentification.Models;
using VoiceAuthentification.Services;

namespace VoiceAuthentification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private string infoMessage = string.Empty;

        private readonly VoiceManager _audioManager;
        private readonly ILogger<VoiceController> _logger;
           
        public VoiceController(ILogger<VoiceController> logger)
        {
            _logger = logger;

            _audioManager = new VoiceManager();
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VoiceData voiceData)
        {
            if (!IsValidateBody()) 
            { 
                return BadRequest(infoMessage); 
            }

            //await GetVoiceStream(Request.Body);
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register()
        {
            infoMessage = "The voice has been successfully analyzed for registration, a refund is being made";
            //if (!IsValidateBody())
            //{
            //    return BadRequest(infoMessage);
            //}

            await _audioManager.SetStreamAudio(Request.Body);

            _logger.LogInformation(infoMessage);
            return Ok();
        }

        private bool IsValidateBody()
        {
            if (this.Request.Body.Length <= 0)
            {
                infoMessage = "Audio stream is empty";
                _logger.LogError(infoMessage);
                return false;
            }
            return true;
        }

    }
}
