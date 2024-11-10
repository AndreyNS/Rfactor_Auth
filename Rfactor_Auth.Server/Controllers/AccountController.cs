using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Rfactor_Auth.Server.Interfaces;
using Rfactor_Auth.Server.Interfaces.Database;
using Rfactor_Auth.Server.Models;
using Rfactor_Auth.Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Rfactor_Auth.Server.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private static List<RefreshToken> refreshTokens = new List<RefreshToken>();

        private readonly ILogger<AccountController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ICrypto _generateManager;
        private readonly IUnitOfWork _context;
        private readonly ICache _cache;

        public AccountController(ILogger<AccountController> logger, ICrypto generateManager, IUnitOfWork context, IConfiguration configuration, ICache cache)
        {
            _cache = cache;
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _generateManager = generateManager;       
        }


        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Models.LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized();
            }

            try
            {
                var user = await _context.UserAuthorizes.FirstOrDefaultAsync(f => f.UserName == request.Username, include => include.UserProfile);
                if (user == null)
                {
                    return NotFound();
                }

                byte[] salt = Convert.FromBase64String(user.Salt);
                if (salt == null || salt.Length == 0)
                {
                    throw new Exception("По какой-то причине соль не была найдена");
                }

                string hash = _generateManager.HashPassword(request.Password, salt);
                if (user.Password != hash)
                {
                    return Unauthorized();
                }

                return JwtToken(request.Username);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(Login)}] Ошибка авторизации");
                return Unauthorized();
            }
        }

        [Authorize]
        [HttpGet("logout")]
        public IActionResult Logout([FromHeader(Name = "Authorization")] string bearer)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(bearer) || !bearer.StartsWith("Bearer "))
                {
                    return BadRequest("Неверный заголовок авторизации");
                }

                var token = bearer.Replace("Bearer ", "");

                _cache.AddTokenToBlacklist(token, 5);
                _logger.LogInformation("Пользователь вышел, токен добавлен в черный список.");
                return Ok(new { message = "Выход успешно выполнен" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(Logout)}] Ошибка при выходе из системы");
                return StatusCode(500, "Произошла ошибка при выходе из системы");
            }
        }


        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] Models.RefreshRequest request)
        {
            var storedRefreshToken = refreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (storedRefreshToken == null)
                return Unauthorized();

            var newAccessToken = _generateManager.GenerateAccessToken(storedRefreshToken.Username);
            var newRefreshToken = _generateManager.GenerateRefreshToken();

            refreshTokens.Remove(storedRefreshToken);
            refreshTokens.Add(new RefreshToken { Token = newRefreshToken, Username = storedRefreshToken.Username });

            return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        }


        [Authorize]
        [HttpGet("authorize")]
        public async Task<IActionResult> Authorize()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized();
                }

                var user = await _context.UserAuthorizes.AnyAsync(f => f.UserName == username);
                if (!user)
                {
                    _logger.LogWarning($"[{nameof(GetAccount)}] Пользователь не найден");
                    return Unauthorized();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(GetAccount)}] Ошибка аутентификации");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        [Authorize]
        [HttpGet("home")]
        public async Task<IActionResult> GetAccount()
        {
            try
            {
                var username = User.Identity?.Name;
                if (string.IsNullOrWhiteSpace(username))
                {
                    return Unauthorized();
                }

                var user = await _context.UserAuthorizes.FirstOrDefaultAsync(
                    f => f.UserName == username,
                    include => include.UserProfile);

                if (user == null)
                {
                    _logger.LogWarning($"[{nameof(GetAccount)}] Пользователь не найден");
                    return Unauthorized();
                }

                dynamic userResp = new
                {
                    UserName = username,
                    Fio = user.UserProfile.FIO,
                    Email = user.UserProfile.Email,

                };

                return Ok(userResp);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(GetAccount)}] Ошибка аутентификации");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.RegisterRequest request)
        {
            var fields = new[] { request.Username, request.Email, request.Fio, request.Password, request.ConfirmPassword };
            if (fields.Any(string.IsNullOrWhiteSpace))
            {
                _logger.LogWarning($"[{nameof(Register)}] Пользовательские данные полностью или частично пусты");
                return BadRequest("Заполните все поля");
            }

            if (request.Password != request.ConfirmPassword)
            {
                _logger.LogWarning($"[{nameof(Register)}] Пароли не совпадают");
                return BadRequest("Пароли не совпадают");
            }

            bool isEmailExist = await _context.UserProfiles.AnyAsync(a => a.Email == request.Email);
            bool isUsernameExist = await _context.UserAuthorizes.AnyAsync(a => a.UserName == request.Username);

            if (isEmailExist || isUsernameExist)
            {
                if (isEmailExist)
                {
                    _logger.LogWarning($"[{nameof(Register)}] Email недоступен");
                    return Conflict("Email недоступен");
                }

                if (isUsernameExist)
                {
                    _logger.LogWarning($"[{nameof(Register)}] Username недоступен");
                    return Conflict("Username недоступен");
                }
            }

            try
            {
                byte[] salt = _generateManager.GenerateSalt();
                string hashPassword = _generateManager.HashPassword(request.Password, salt);

                Guid guid = Guid.NewGuid();
                UserAuthorize authorize = new()
                {
                    Guid = guid,
                    UserName = request.Username,
                    Password = hashPassword,
                    Salt = Convert.ToBase64String(salt)
                };

                await _context.UserAuthorizes.AddAsync(authorize);

                UserProfile profile = new()
                {
                    UserGuid = guid,
                    FIO = request.Fio,
                    Email = request.Email,
                };

                await _context.UserProfiles.AddAsync(profile);
                await _context.SaveChangesAsync();

                return JwtToken(request.Username);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(Register)}] Ошибка авторизации");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private IActionResult JwtToken(string userName)
        {
           
            var accessTokenString = _generateManager.GenerateAccessToken(userName);
            var refreshToken = _generateManager.GenerateRefreshToken();

            return Ok(new
            {
                accessToken = accessTokenString,
                refreshToken = refreshToken
            });
        }

    }
}
