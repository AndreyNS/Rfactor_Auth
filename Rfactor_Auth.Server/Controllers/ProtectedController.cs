using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rfactor_Auth.Server.Interfaces;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        private readonly ILogger<ProtectedController> _logger;
        public ProtectedController(ILogger<ProtectedController> logger)
        {
            _logger = logger;
        }

        [HttpGet("initiate/{method}")]
        public IActionResult InitiateVoiceAuth([FromRoute] string method, [FromQuery] string username)
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action(nameof(HandleLogin), "Protected", null, Request.Scheme)
            };
            authenticationProperties.Items["acr_values"] = method;
            authenticationProperties.Items["username"] = username;

            _logger.LogInformation($"[{nameof(InitiateVoiceAuth)}] RedirectUri {authenticationProperties.RedirectUri}.");
            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("callback")]
        public async Task<IActionResult> HandleLogin()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();
            if (!authenticateResult.Succeeded)
            {
                return BadRequest("Authentication failed.");
            }

            var accessToken = await HttpContext.GetTokenAsync("access_token");
            var idToken = await HttpContext.GetTokenAsync("id_token");

            _logger.LogInformation($"[{nameof(HandleLogin)}] Authentication successful.");
            return Ok("Authentication successful.");
        }
    }
}
