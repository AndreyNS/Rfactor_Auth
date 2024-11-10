using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityServer.Interfaces.Database;
using IdentityServer.Models;
using IdentityServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using System;
using static System.Net.Mime.MediaTypeNames;

namespace IdentityServer.Controllers
{
    [Route("oauth2/v2")]
    public class AuthorizeController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<AuthorizeController> _logger;
        private readonly IUnitOfWork _context;

        public AuthorizeController(IIdentityServerInteractionService interaction, ILogger<AuthorizeController> logger, IUnitOfWork context)
        {
            _interaction = interaction;
            _logger = logger;
            _context = context;
        }

        [HttpGet("auth")]
        public async Task<IActionResult> Authorize(string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(returnUrl))
            {
                return View("Error");
            }

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (context == null)
            {
                return View("Error");
            }

            var clientId = context.Client.ClientId;
            var redirectUri = context.RedirectUri;
            var responseType = context.Parameters["response_type"];
            var scope = context.ValidatedResources.RawScopeValues;
            var state = context.Parameters["state"];
            var acrValues = context.Parameters["acr_values"];
            var username = context.Parameters["username"]; ;

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri) ||
                string.IsNullOrWhiteSpace(responseType) || !scope.Any() ||
                string.IsNullOrWhiteSpace(state) || string.IsNullOrWhiteSpace(acrValues) ||
                string.IsNullOrWhiteSpace(username))
            {
                return View("Error");
            }

            if (!await _context.UserAuthorizes.AnyAsync(a => a.UserName == username))
            {
                await _context.UserAuthorizes.AddAsync(new UserAuthorize { UserName = username });
                await _context.SaveChangesAsync();
            }

            var user = await _context.UserAuthorizes.FirstOrDefaultAsync(f => f.UserName == username);
            if (user == null)
            {
                return View("Error");
            }

            IActionResult action;
            var authenticationMethods = new Dictionary<string, Func<IActionResult>>
                {
                    { "voice", () => user.IsVoiceAuthentifation ? RedirectBack(redirectUri) : HandleAuthenticationView("Voice", user.Guid, redirectUri, state)},
                    { "image", () => user.IsImageAuthentifation ? RedirectBack(redirectUri) : HandleAuthenticationView("Image", user.Guid, redirectUri, state, context.Parameters["image"]) },
                    { "sphere", () => user.IsSphereAuthentifation ? RedirectBack(redirectUri) : HandleAuthenticationView("Sphere", user.Guid, redirectUri, state) },
                    { "environment", () => user.IsEnvironmentAuthentifation ? RedirectBack(redirectUri) : HandleAuthenticationView("Environment", user.Guid, redirectUri, state) },
                    { "odometry", () => user.IsOdomentryAuthentifation ? RedirectBack(redirectUri) : HandleAuthenticationView("Odometry", user.Guid, redirectUri, state) }
                };

            action = authenticationMethods.TryGetValue(acrValues, out var method) ? method() : View("Error");

            return action;
        }

        private IActionResult RedirectBack(string redirectUrl)
        {
            return Redirect(redirectUrl);
        }

        private IActionResult HandleAuthenticationView(string methodName, Guid guid, string redirectUrl, string state, string? image = null)
        {
            var viewName = methodName switch
            {
                "Image" => image == "56912031" ? "ImageSet" : "ImageCheck",
                _ => methodName
            };
            return View(viewName, new AuthViewModel { Guid = guid, ReturnUrl = redirectUrl, State = state });
        }

    }
}
