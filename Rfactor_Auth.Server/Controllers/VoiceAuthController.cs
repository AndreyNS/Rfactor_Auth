using MediaToolkit.Model;
using MediaToolkit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using NAudio.Wave;
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
        public async Task<IActionResult> ProcessVoiceAuth(IFormFile voice)
        {
            if (voice == null || voice.Length == 0)
            {
                return BadRequest(new { Message = "Файл не загружен или пустой" });
            }

            string tempFilePath = Path.Combine(Directory.GetCurrentDirectory(), "temp.webm");

            using (var fileStream = System.IO.File.Create(tempFilePath))
            {
                voice.CopyTo(fileStream);
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "goosy.wav");

            var inputMediaFile = new MediaFile { Filename = tempFilePath };
            var outputMediaFile = new MediaFile { Filename = filePath };

            using (var engine = new Engine())
            {
                engine.Convert(inputMediaFile, outputMediaFile);
            }

            //System.IO.File.Delete(tempFilePath);


            

            using (var memoryStream = new MemoryStream())
            {
                await voice.CopyToAsync(memoryStream);

                var waveFormat = new WaveFormat(44100, 2); 
                using (var waveFileWriter = new WaveFileWriter(filePath, waveFormat))
                {
                    memoryStream.Position = 0;

                    waveFileWriter.Write(memoryStream.ToArray(), 0, (int)memoryStream.Length);
                }
            }


            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await voice.CopyToAsync(memoryStream);
                    var audioBytes = memoryStream.ToArray();

                    using var content = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(audioBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                    content.Add(fileContent, "voice", voice.FileName);

                    var response = await _httpClient.PostAsync("Voice/register", content);
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
