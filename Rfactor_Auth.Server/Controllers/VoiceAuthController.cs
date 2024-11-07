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
using Rfactor_Auth.Server.Interfaces;
using Rfactor_Auth.Server.Services;
using System;
using Newtonsoft.Json;
using System.Text;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceAuthController : ControllerBase
    {
        private readonly VoiceConverterBase _converter;
        private readonly ILogger<VoiceAuthController> _logger;
        private HttpClient _httpClient;
        public VoiceAuthController(ILogger<VoiceAuthController> logger, IHttpClientFactory httpClientFactory, VoiceConverterBase converter)
        {
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient("VoiceAuth");
            _converter = converter;

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

            //using (var client = new HttpClient())
            //{пу
            //    var bytephoto = System.IO.File.ReadAllBytes(@"C:\Users\az674\Downloads\response.png"); 
            //    var base64Image = Convert.ToBase64String(bytephoto); 
            //    var content = new StringContent(JsonConvert.SerializeObject(new { image = base64Image }), Encoding.UTF8, "application/json"); 
            //    var response = await client.PostAsync("http://178.46.160.94/api/check/", content); 
                
            //    if (response.IsSuccessStatusCode) 
            //    { 
            //        var contentJson = await response.Content.ReadAsStringAsync(); 
            //        Console.WriteLine(contentJson); 
            //    } 
            //    else 
            //    { 
            //        Console.WriteLine("Ошибка: " + response.StatusCode); 
            //    }
            //}


            if (voice == null || voice.Length == 0)
            {
                return BadRequest(new { Message = "Файл не загружен или пустой" });
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await voice.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    _converter.TurnSaver(false); // Не забыть поставить false
        
                    Stream convertedVoiceStream = await _converter.Convert(memoryStream);
                    memoryStream.SetLength(0);

                    await convertedVoiceStream.CopyToAsync(memoryStream);
                    var audioBytes = memoryStream.ToArray();
                    convertedVoiceStream.Close();

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
