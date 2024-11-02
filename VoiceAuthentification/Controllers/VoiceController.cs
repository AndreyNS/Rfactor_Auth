using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VoiceAuthentification.Models;

namespace VoiceAuthentification.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceController : ControllerBase
    {
        [HttpPost(Name = "verify")]
        public async Task<IActionResult> VerifyVoice([FromBody] VoiceData voiceData)
        {
            return Unauthorized();
        }
    }
}
