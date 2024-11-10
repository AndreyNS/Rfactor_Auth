using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using IdentityModel;
using IdentityServer.Interfaces;
using IdentityServer.Interfaces.Database;
using IdentityServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace IdentityServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoiceAuthController : ControllerBase
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly ILogger<VoiceAuthController> _logger;
        private readonly VoiceConverterBase _converter;
        private readonly IUnitOfWork _context;
        private HttpClient _httpClient;

        public VoiceAuthController(ILogger<VoiceAuthController> logger, IHttpClientFactory httpClientFactory, VoiceConverterBase converter,IUnitOfWork context, IIdentityServerInteractionService interaction)
        {
        
            _httpClient = httpClientFactory.CreateClient("VoiceAuth");
            _interaction = interaction;
            _converter = converter;
            _context = context;
            _logger = logger;
        }


        [HttpPost("set")]
        public async Task<IActionResult> ProcessVoiceAuth([FromForm] IFormFile voice, [FromQuery] Guid guid, [FromQuery] string redirect_uri, [FromQuery] string state)
        {
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            if (!IPAddress.IsLoopback(remoteIp))
            {
                return Unauthorized("Отказ в доступе");
            }
            if (voice == null || voice.Length == 0)
            {
                return BadRequest(new { Message = "Файл не загружен или пустой" });
            }

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    await voice.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;

                    _converter.TurnSaver(false); // Не забыть поставить false
        
                    Stream convertedVoiceStream = await _converter.Convert(memoryStream);
                    memoryStream.SetLength(0);

                    await convertedVoiceStream.CopyToAsync(memoryStream);
                    var audioBytes = memoryStream.ToArray();
                    convertedVoiceStream.Close();

                    using var content = new MultipartFormDataContent();
                    var fileContent = new ByteArrayContent(audioBytes);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                    content.Add(fileContent, "voice", voice.FileName);

                    var response = await _httpClient.PostAsync("api/register", content);
                    if (response.IsSuccessStatusCode)
                    {
                        string responseJson = await  response.Content.ReadAsStringAsync();

                        if (string.IsNullOrWhiteSpace(responseJson))
                        {
                            throw new Exception("Данные голоса пусты");
                        }

                        await _context.VoiceDataset.AddAsync(new VoiceData { Voice = responseJson, UserGuid = guid });
                        //var user = await _context.UserAuthorizes.FirstOrDefaultAsync(f => f.Guid == guid);
                        //user.IsVoiceAuthentifation = true;

                        //await _context.SaveChangesAsync();

                        //_logger.LogInformation($"[{nameof(ProcessVoiceAuth)}] Авторизация голосом успешно установлена.");
                        //return Redirect(redirect_uri);

                        var user = await _context.UserAuthorizes.FirstOrDefaultAsync(f => f.Guid == guid);
                        if (user == null)
                        {
                            return BadRequest(new { Message = "Пользователь не найден." });
                        }

                        user.IsVoiceAuthentifation = true;
                        await _context.SaveChangesAsync();

                        var claims = new List<Claim>
                        {
                            new Claim(JwtClaimTypes.Subject, user.Guid.ToString()),
                            new Claim(JwtClaimTypes.Name, user.UserName)
 
                        };

                        var isuser = new IdentityServerUser(user.Guid.ToString())
                        {
                            DisplayName = user.UserName,
                            IdentityProvider = IdentityServerConstants.LocalIdentityProvider,
                            AuthenticationTime = DateTime.UtcNow,
                            AdditionalClaims = claims
                        };

                        await HttpContext.SignInAsync(isuser);
                        _logger.LogInformation($"[{nameof(ProcessVoiceAuth)}] Аутент голосом успешно установлена для юзера {user.UserName}.");


                        return Redirect($"{redirect_uri}?state={state}");
                        //return Ok(new { Message = "Аутентификация успешна", RedirectUri = redirect_uri });

                    }

                    _logger.LogCritical($"[{nameof(ProcessVoiceAuth)}] Не удалось установить образец голоса.");
                    return BadRequest(new { Message = "Не удалось установить образец голоса." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(ProcessVoiceAuth)}] Ошибка обработки голоса.");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
