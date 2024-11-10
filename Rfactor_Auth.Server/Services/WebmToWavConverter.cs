using MediaToolkit.Model;
using MediaToolkit;
using Rfactor_Auth.Server.Interfaces;
using System;

namespace Rfactor_Auth.Server.Services
{
    public class WebmToWavConverter : VoiceConverterBase
    {
        private string currentDirectory = Directory.GetCurrentDirectory();

        private readonly ILogger<WebmToWavConverter> _logger;
        private bool isSave = false;

        public WebmToWavConverter(ILogger<WebmToWavConverter> logger)
        {
            _logger = logger;
        }

        public override async Task<Stream> Convert(Stream voice)
        {
            Guid guid = Guid.NewGuid();

            string tempFilePath = Path.Combine(currentDirectory, $"{guid}.webm");
            string filePath = Path.Combine(currentDirectory, $"{guid}.wav");

            try
            {
                using (var fileStream = File.Create(tempFilePath))
                {
                    voice.CopyTo(fileStream);
                }

                var inputMediaFile = new MediaFile { Filename = tempFilePath };
                var outputMediaFile = new MediaFile { Filename = filePath };

                using (var engine = new Engine())
                {
                    engine.Convert(inputMediaFile, outputMediaFile);
                }

                Stream convertedFileStream = File.Open(filePath, FileMode.Open);

                MemoryStream memoryStream = new MemoryStream();
                convertedFileStream.CopyTo(memoryStream);
                memoryStream.Position = 0;

                convertedFileStream.Close();

                if (!isSave)
                {
                    File.Delete(tempFilePath);
                    File.Delete(filePath);
                }
                else
                {
                    isSave = !isSave;
                }
                _logger.LogInformation($"[{nameof(Convert)}] Конвертация (webm to wav) проведена успешно.");
                return memoryStream;
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(Convert)}] Ошибка конвертации файла.");
                throw;
            }
        }

        public override void TurnSaver(bool isSave)
        {
            this.isSave = isSave;
        }
    }
}
