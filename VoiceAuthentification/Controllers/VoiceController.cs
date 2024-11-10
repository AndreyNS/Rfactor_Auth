using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using VoiceAuthentification.Interface;
using VoiceAuthentification.Models;
using VoiceAuthentification.Services;

namespace VoiceAuthentification.Controllers
{
    [Route("api")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        private string infoMessage = string.Empty;

        private readonly IVoiceManager _audioManager;
        private readonly ILogger<VoiceController> _logger;
           
        public VoiceController(ILogger<VoiceController> logger, IVoiceManager voiceManager)
        {
            _logger = logger;
            _audioManager = voiceManager;
        }

        //[HttpPost("verify")]
        //public async Task<IActionResult> Verify([FromBody] VoiceData voiceData)
        //{
        //    //if (!IsValidateBody()) 
        //    //{ 
        //    //    return BadRequest(infoMessage); 
        //    //}

        //    //await GetVoiceStream(Request.Body);
        //    return Unauthorized();
        //}

        [HttpPost("register")]
        public async Task<IActionResult> Register(IFormFile voice)
        {
            infoMessage = "Голос был успешно проанализирован для регистрации";

            try
            {
                using var ms = new MemoryStream();
                await voice.CopyToAsync(ms);

                if (!IsValidateBody(ms))
                {
                    return BadRequest(infoMessage);
                }

                ms.Position = 0;

                await _audioManager.SetStreamAudio(ms);
                ms.Close();
                await _audioManager.VoiceProcessAsync();

                var body = _audioManager.GetVoiceData();

                if (body == null)
                {
                    throw new Exception("Данные о голосе пусты");
                }
                _logger.LogInformation($"[{nameof(Register)}] {infoMessage}");
                return Ok(body);
            }
            catch (Exception ex)
            {
                infoMessage = "Ошибка при регистрации голоса";

                _logger.LogError(ex, $"[{nameof(Register)}] {infoMessage}");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private bool IsValidateBody(Stream voice)
        {
            if (voice.CanSeek)
            {
                if (voice.Length == 0)
                {
                    infoMessage = "Audio stream is empty";
                    _logger.LogError(infoMessage);
                    return false;
                }
            }
            return true;
        }

    }
}
