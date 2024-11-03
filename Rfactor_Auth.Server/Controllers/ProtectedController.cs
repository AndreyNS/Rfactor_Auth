using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectedController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var userName = User.Identity.Name;
            return Ok(new { Message = $"Привет, {userName}! Это защищенный ресурс." });
        }

    }
}
