using NAudio.Wave;
using System.IO;
using System.Net.Http.Headers;
using VoiceAuthentification.Interface;
using Vosk;

namespace VoiceAuthentification.Services
{
    public class SpeechRecognition : IRecognition
    {
        private readonly ILogger<SpeechRecognition> _logger;
        private readonly HttpClient _httpClient;
        public SpeechRecognition(ILogger<SpeechRecognition> logger, IHttpClientFactory httpClient)
        {
            _logger = logger;
            _httpClient = httpClient.CreateClient("RecognitionSpeech");
        }

        public async Task<string> RecognizeSpeech(byte[] audioBytes)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new ByteArrayContent(audioBytes);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

                content.Add(fileContent, "voice", "UserVoice");

                var response = await _httpClient.PostAsync("api/recognition", content);
                    if (response.IsSuccessStatusCode)
                {
                    string phrase = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(phrase))
                    {
                        throw new Exception($"[{nameof(RecognizeSpeech)}] Фраза пуста, не удалось распознать");
                    }

                    _logger.LogInformation($"[{nameof(RecognizeSpeech)}] Фраза возвращена назад");
                    return phrase;
                }

                throw new Exception($"[{nameof(RecognizeSpeech)}] Ошибка сервиса распознавания голоса, [{response.StatusCode}]");
            }
            catch (AccessViolationException ex)
            {
                _logger.LogError(ex, $"[{nameof(RecognizeSpeech)}] Access violation");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(RecognizeSpeech)}] Ошибка распознавателя голоса");
                throw;
            }
        }
    }
}


