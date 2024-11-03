using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    [Route("oauth2/v2")]
    public class AuthorizeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;


        public AuthorizeController(IIdentityServerInteractionService interaction, IEventService events)
        {
            _interaction = interaction;
        }


        [HttpGet("auth")]
        public async Task<IActionResult> Authorize(string client_id, string redirect_uri, string response_type, string scope, string state)
        {
            if (string.IsNullOrEmpty(client_id))
            {
                return View("Error");
            }

            //var validClientId = _configuration["OAuth:ClientId"];
            //if (client_id != validClientId)
            //{
            //    return Unauthorized("Invalid client_id.");
            //}

            if (string.IsNullOrEmpty(redirect_uri) || string.IsNullOrEmpty(response_type) || string.IsNullOrEmpty(scope))
            {
                return BadRequest("Missing required parameters.");
            }
            return View(new LoginInputModel());
        }
    }

}
