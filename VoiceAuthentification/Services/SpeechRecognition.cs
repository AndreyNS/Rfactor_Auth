using NAudio.Wave;
using System.IO;
using Vosk;

namespace VoiceAuthentification.Services
{
    public class SpeechRecognition
    {
        public async Task RecognizeSpeechFromBytes(byte[] audioByte, int sampleRate = 16000)
        {
            try
            {
                await Task.Run(() =>
                {
                    var filePath = @"F:\Phone_ARU_OFF.wav";
                    var audioBytes = File.ReadAllBytes(filePath);

                    Vosk.Vosk.SetLogLevel(1);

                    Model _model = new(@"F:\vosk-model-ru-0.42");

                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    using var stream = new MemoryStream(audioByte);

                    Stream streamConv = ResampleTo16kHz(stream);
                    using var rawStream = new RawSourceWaveStream(streamConv, new WaveFormat(sampleRate, 1));
                    var recognizer = new VoskRecognizer(_model, sampleRate); 

                    while ((bytesRead = streamConv.Read(buffer, 0, buffer.Length)) > 0)
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
            catch (AccessViolationException ex)
            {
                Console.WriteLine($"Access violation: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        private Stream ResampleTo16kHz(Stream inputStream)
        {
            var waveStream = new WaveFileReader(inputStream);
            var resampler = new MediaFoundationResampler(waveStream, new WaveFormat(16000, waveStream.WaveFormat.Channels))
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


