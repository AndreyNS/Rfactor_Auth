using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rfactor_Auth.Server.Models;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceAuthController : ControllerBase
    {
        private readonly ILogger<VoiceAuthController> _logger; 
        public VoiceAuthController(ILogger<VoiceAuthController> logger) 
        { 
            _logger = logger; 
        }

        [HttpPost("initiate")] 
        public IActionResult InitiateVoiceAuth() 
        { 
            var authenticationProperties = new AuthenticationProperties 
            { 
                RedirectUri = Url.Action(nameof(HandleLogin)) 
            }; 
            _logger.LogInformation($"[{nameof(VoiceAuthController)}] Initiating voice authentication."); 
            return Challenge(authenticationProperties, "RfactorVoice"); }

        [HttpGet("callback")] 
        public async Task<IActionResult> HandleLogin() 
        { 
            var code = Request.Query["code"]; 
            if (string.IsNullOrEmpty(code)) 
            { 
                return BadRequest("Authorization code is missing."); 
            } 
            //var tokenResponse = await GetTokenAsync(code); 
            //if (tokenResponse == null) 
            //{ 
            //    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get token."); 
            //} 
            
            _logger.LogInformation($"[{nameof(VoiceAuthController)}] Voice authentication successful."); 
            return Ok("Voice authentication successful."); }


        [HttpPost]
        public async Task<IActionResult> ProcessVoiceAuth([FromBody] VoiceData voiceData)
        {
            if (voiceData == null || string.IsNullOrWhiteSpace(voiceData.VoiceBase64))
            {
                return BadRequest(new
                {
                    Message = "Voice Data empty"
                });
            }

            var result = await SendAudioToVoiceService(voiceData.VoiceBase64);
            if (result)
            {
                return Ok(new { Message = "Voice authentication successful" });
            }
            return BadRequest(new
            {
                Message = "Voice authentication failed"
            });
        }

        private async Task<bool> SendAudioToVoiceService(string audioUri)
        {
            return true;
        }
    }
}
