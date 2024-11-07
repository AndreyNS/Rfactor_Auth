using Microsoft.VisualBasic;
using NAudio.Wave;
using System.Reflection;
using Vosk;
using VoskRecognition.Interfaces;

namespace VoskRecognition.Services
{
    public class RecognitionManager : IRecogtion
    {
        private const string PathModel = @"F:\vosk-model-ru-0.42";
        private const int SampleRate = 16000;
        private const int VoskLogLevel = 1;

        private readonly ILogger<RecognitionManager> _logger;
        private readonly VoskRecognizer _recognizer;
        private readonly Model _model;

        public RecognitionManager(ILogger<RecognitionManager> logger)
        {
            _logger = logger;

            _model = new(PathModel);
            _recognizer = new VoskRecognizer(_model, SampleRate);

            Vosk.Vosk.SetLogLevel(VoskLogLevel);
        }

        public async Task<string> RecognizeSpeech(Stream stream)
        {
            try
            {
                byte[] buffer = new byte[4096];
                int bytesRead;

                Stream streamConv = ResampleTo16kHz(stream);
                while ((bytesRead = streamConv.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (_recognizer.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine("Распознанная фраза: " + _recognizer.Result());
                    }
                    else
                    {
                        Console.WriteLine("Частичный результат: " + _recognizer.PartialResult());
                    }
                }

                _logger.LogInformation("Окончательный результат: " + _recognizer.FinalResult());
                return _recognizer.FinalResult();

            }
            catch (AccessViolationException ex)
            {
                _logger.LogError($"Access violation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Unexpected error: {ex.Message}");
                throw;
            }
        }

        private Stream ResampleTo16kHz(Stream inputStream)
        {
            var waveStream = new WaveFileReader(inputStream);
            var resampler = new MediaFoundationResampler(waveStream, new WaveFormat(waveStream.WaveFormat.SampleRate, waveStream.WaveFormat.Channels))
            {
                ResamplerQuality = 60
            };

            var outputStream = new MemoryStream();
            WaveFileWriter.WriteWavFileToStream(outputStream, resampler);

            outputStream.Position = 0;
            return outputStream;
        }
    }
}
