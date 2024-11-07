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
    [Route("api/[controller]")]
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

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VoiceData voiceData)
        {
            //if (!IsValidateBody()) 
            //{ 
            //    return BadRequest(infoMessage); 
            //}

            //await GetVoiceStream(Request.Body);
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(IFormFile voice)
        {
            infoMessage = "The voice has been successfully analyzed for registration, a refund is being made";

            using var ms = new MemoryStream();
            await voice.CopyToAsync(ms);

            if (!IsValidateBody(ms))
            {
                return BadRequest(infoMessage);
            }

            ms.Position = 0;

            //using (var memoryStream = new MemoryStream())
            //{
            //    await voice.CopyToAsync(memoryStream);
            //    var audioBytes = memoryStream.ToArray();

            //    memoryStream.Position = 0;


            //    var waveFormat = new WaveFormat(16000, 1); // Частота и моно
            //    using var waveFileWriter = new WaveFileWriter(filePath, waveFormat);
            //    waveFileWriter.Write(audioBytes, 0, audioBytes.Length);
            //}


            await _audioManager.SetStreamAudio(ms);
            ms.Close();

            await _audioManager.VoiceProcessAsync();

            _logger.LogInformation(infoMessage);
            return Ok();
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
