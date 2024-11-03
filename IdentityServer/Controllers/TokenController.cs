using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    [Route("oauth2/v2")]
    public class TokenController : Controller
    {
        [HttpPost("token")]
        public IActionResult Token()
        {
            return Ok(new { access_token = "token_value", token_type = "bearer" });
        }
    }
}
