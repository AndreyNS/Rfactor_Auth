using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rfactor_Auth.Server.Models;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceAuthController : ControllerBase
    {
        private readonly ILogger<VoiceAuthController> _logger;
        private HttpClient _httpClient;
        public VoiceAuthController(ILogger<VoiceAuthController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("VoiceAuth");
        }

        [HttpGet("initiate")]
        public IActionResult InitiateVoiceAuth()
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(HandleLogin))
            };
            _logger.LogInformation($"[{nameof(VoiceAuthController)}] Initiating voice authentication.");
            return Challenge(authenticationProperties, "IdentityServer");
        }

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
            return Ok("Voice authentication successful.");
        }

        [HttpPost("setvoice")]
        public async Task<IActionResult> ProcessVoiceAuth()
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await Request.Body.CopyToAsync(memoryStream);
                    var audioBytes = memoryStream.ToArray();

                    var requestContent = new ByteArrayContent(audioBytes); 
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
                    var response = await _httpClient.PostAsync("Voice/register", requestContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return Ok(new { Message = "Voice authentication successful" });
                    }

                    return BadRequest(new { Message = "Voice authentication failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing voice authentication.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error.");
            }
        }
    }
}
