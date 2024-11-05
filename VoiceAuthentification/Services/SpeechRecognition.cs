using NAudio.Wave;
using System.IO;
using Vosk;

namespace VoiceAuthentification.Services
{
    public class SpeechRecognition
    {
        public async Task RecognizeSpeechFromBytes(byte[] audioByte, int sampleRate = 16000)
        {
            //var filePath = Path.Combine(Directory.GetCurrentDirectory(), "goosy.wav");

            //var waveFormat = new WaveFormat(sampleRate, 1); // Частота и моно

            //using var memoryStream = new MemoryStream(audioByte);
            //using var waveFileWriter = new WaveFileWriter(filePath, waveFormat);

            //// Записываем данные из audioByte в файл WAV
            //waveFileWriter.Write(audioByte, 0, audioByte.Length);

            await Task.Run(() =>
            {
                Vosk.Vosk.SetLogLevel(1);

                Model _model = new(@"F:\vosk-model-ru-0.42");
 
                byte[] buffer = new byte[4096];
                int bytesRead;

                using var stream = new MemoryStream(audioByte);
                using var rawStream = new RawSourceWaveStream(stream, new WaveFormat(sampleRate, 1)); // 16 кГц, моно
                var recognizer = new VoskRecognizer(_model, sampleRate); // Убедитесь, что модель также на 16 кГц

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (recognizer.AcceptWaveform(buffer, bytesRead))
                    {
                        Console.WriteLine("Распознанная фраза: " + recognizer.Result());
                    }
                    else
                    {
                        Console.WriteLine("Частичный результат: " + recognizer.PartialResult());
                    }
                }
                Console.WriteLine("Окончательный результат: " + recognizer.FinalResult());
            }).ConfigureAwait(false);
        }
    }
}
