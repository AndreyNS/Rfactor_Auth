using Microsoft.VisualBasic;
using NAudio.Wave;
using System.Diagnostics;
using System.Reflection;
using Vosk;
using VoskRecognition.Interfaces;

namespace VoskRecognition.Services
{
    public class RecognitionManager : IRecogtion
    {
        private string dirPath = Directory.GetCurrentDirectory();
        private const string PathModel = @"VoskModel/vosk-model-ru-0.42";
        private const int SampleRate = 16000;
        private const int VoskLogLevel = 1;

        private readonly ILogger<RecognitionManager> _logger;
        private readonly VoskRecognizer _recognizer;
        private readonly Model _model;

        private string phrase = string.Empty;

        public RecognitionManager(ILogger<RecognitionManager> logger)
        {
            _logger = logger;
            dirPath = Path.Combine(dirPath, PathModel);

            try
            {
                _model = new(dirPath);
                _recognizer = new VoskRecognizer(_model, SampleRate);

                Vosk.Vosk.SetLogLevel(VoskLogLevel);
                _logger.LogInformation($"[{nameof(RecognitionManager)}] Vosk успешно запущен");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{nameof(RecognitionManager)}] Ошибка модели Vosk");
            }

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
                        Console.WriteLine("распознанная фраза: " + _recognizer.Result());
                    }
                    else
                    {
                        Console.WriteLine("частичный: " + _recognizer.PartialResult());
                    }
                }

                phrase = _recognizer.FinalResult();
                _logger.LogInformation($"[{nameof(RecognizeSpeech)}] Фулл фраза: " + phrase);
                return phrase;

            }
            catch (AccessViolationException ex)
            {
                _logger.LogError($"[{nameof(RecognizeSpeech)}] Access violation: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"[{nameof(RecognizeSpeech)}] Unexpected error: {ex.Message}");
                throw;
            }
        }

        //private Stream ResampleTo16kHz(Stream inputStream)
        //{
        //    var waveStream = new WaveFileReader(inputStream);
        //    var resampler = new MediaFoundationResampler(waveStream, new WaveFormat(waveStream.WaveFormat.SampleRate, waveStream.WaveFormat.Channels))
        //    {
        //        ResamplerQuality = 60
        //    };

        //    var outputStream = new MemoryStream();
        //    WaveFileWriter.WriteWavFileToStream(outputStream, resampler);

        //    outputStream.Position = 0;
        //    return outputStream;
        //}

        private Stream ResampleTo16kHz(Stream inputStream)
        {
            var inputFilePath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");
            var outputFilePath = Path.ChangeExtension(Path.GetTempFileName(), ".wav");

            try
            {
                using (var fileStream = new FileStream(inputFilePath, FileMode.Create, FileAccess.Write))
                {
                    inputStream.CopyTo(fileStream);
                }

                var ffmpeg = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = $"-i {inputFilePath} -ar 16000 -ac 1 {outputFilePath}",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                ffmpeg.Start();
                string errorOutput = ffmpeg.StandardError.ReadToEnd();
                ffmpeg.WaitForExit();

                if (ffmpeg.ExitCode != 0)
                {
                    throw new InvalidOperationException("FFmpeg failed to process the audio.");
                }

                var outputStream = new MemoryStream();
                using (var fileStream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(outputStream);
                }

                outputStream.Position = 0;
                return outputStream;
            }
            finally
            {
                if (File.Exists(inputFilePath)) File.Delete(inputFilePath);
                if (File.Exists(outputFilePath)) File.Delete(outputFilePath);
            }
        }
    }
}
