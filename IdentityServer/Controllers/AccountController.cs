using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;

        public AccountController(IIdentityServerInteractionService interaction, IEventService events)
        {
            _interaction = interaction;
            _events = events;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginInputModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("VoiceAuth", new { returnUrl = model.ReturnUrl, username = model.Username });
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VoiceAuth(string returnUrl, string username)
        {
            return View(new VoiceAuthViewModel { ReturnUrl = returnUrl, Username = username });
        }

        [HttpPost]
        public async Task<IActionResult> VoiceAuth(Models.VoiceAuthInputModel model)
        {
            bool isVerified = await VoiceAuthService.VerifyVoiceAsync(model.Username, model.VoiceData);

            if (isVerified)
            {
                //await HttpContext.SignInAsync();

                //await _events.RaiseAsync(new UserLoginSuccessEvent(model.Username, , model.Username));
                return Redirect(model.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError("", "Голосовая аутентификация не пройдена.");
                return View(model);
            }
        }
    }

}
