using VoiceAuthentification.AudioHandlers;
using System.Drawing;
using System.Numerics;
using NAudio.Wave;
using Microsoft.AspNetCore.Http.HttpResults;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics;
using NWaves.Signals;
using NWaves.Transforms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using NWaves.Windows;
using NWaves.FeatureExtractors;
using NWaves.Filters.Base;
using VoiceAuthentification.Interface;
using NWaves.Features;
using NWaves.Utils;
using Microsoft.Extensions.Logging;
using VoiceAuthentification.Models;

namespace VoiceAuthentification.Services
{
    public class VoiceManager : IVoiceManager, IDisposable
    {
        private const int SampleRate = 44100; // частота дискретки
        private const int FrameSize = 2048; // размер фрейма (кадра) анализа
        private const int HopSize = 1024; // шаг анализа
        private const int LpcOrder = 16; // lpc анализа
        private const int HighFrequency = 300; // макс. анализируемая частота
        private const int LowFrequency = 70; // минимальная
        private const double EnergyThreshold = 0.5; // пороговая энергия фрейма (донастроить бы)

        private readonly IRecognition _speechRecognition;
        private readonly ILogger<VoiceManager> _logger;
        private readonly string _decryptedKey;

        private byte[] AudioBytes { get; set; }
        private float[] Spectrum { get; set; }
        private VoiceDataRaw ResponseData { get; set; }
        private DiscreteSignal DiscreteSignal { get; set; }
        
        public VoiceManager(ILogger<VoiceManager> logger, IRecognition recognition, EncryptionService encryptionService)
        {
            _decryptedKey = encryptionService.GetDecryptedKey();
            _speechRecognition = recognition;
            _logger = logger;
        }

        public async Task SetStreamAudio(Stream stream) => AudioBytes = await GetVoiceByteArrayAsync(stream);
        public async Task VoiceProcessAsync()
        {
            try
            {
                ResponseData = new();

                string phrase = await _speechRecognition.RecognizeSpeech(AudioBytes);
                _logger.LogInformation("Точка распознавания речи прошла успешно");

                ResponseData.Phrase = phrase;

                await AnalyzeAsync(); // Создание спектра
                _logger.LogInformation("Точка определения спектра пройдена успешно");

                await ExtractPitch(); // Частота основного тона
                _logger.LogInformation("Точка определения основного тона пройдена успешно");

                await ExtractGlobalFormants(); // Извлечение формантных частот
                _logger.LogInformation("Точка извлечения формантных частот пройдена успешно");

                await AnalyzeRhythm(); // Анализ длительности слова
                _logger.LogInformation("Точка анализ длительности слова пройдена успешно");


                // await ExtractFormants();
                //await ExtractCepstrum(); // Кэпстральные коэф (нестабильно)

                // Deprecated 
                //await ComputeAverageSlope(); // Средний наклон спектра (нестабильно), теперь стабильно, но эффекта почти нет, для одной фразы спектр схож даже на разных частотах, поэтому наклон будет един
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex, $"[{nameof(VoiceProcessAsync)}] Error process analyze");
                return;
            }
        }

        public string GetVoiceData() => ResponseData.ToEncryptedString(_decryptedKey);

        private async Task AnalyzeAsync()
        {
            var voice = new MemoryStream(AudioBytes);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "goosy.wav");
            using (var stream = System.IO.File.Open(filePath, FileMode.Create))
            {
                await voice.CopyToAsync(stream);
            }

            int floatCount = AudioBytes.Length / sizeof(short);
            float[] samples = new float[floatCount];

            for (int i = 0; i < floatCount; i++)
            {
                short pcmSample = BitConverter.ToInt16(AudioBytes, i * sizeof(short));
                samples[i] = pcmSample / 32768f;
            }

            DiscreteSignal = new DiscreteSignal(SampleRate, samples);
            Spectrum = samples;
        }

        private async Task ExtractPitch()
        {
            int samplingRate = DiscreteSignal.SamplingRate;

            var pitchExtractor = new PitchExtractor(new NWaves.FeatureExtractors.Options.PitchOptions
            {
                SamplingRate = samplingRate,
                FrameSize = 4096,
                HopSize = 512,
                HighFrequency = HighFrequency,
                LowFrequency = LowFrequency,
            });

            var pitches = pitchExtractor.ComputeFrom(DiscreteSignal);
            List<float> pitchContour = pitches.Select(pitchArray => pitchArray[0]).Where(p => p > 0).ToList();

            pitchContour = pitchContour.Where(p => p >= LowFrequency && p <= HighFrequency).ToList();
            pitchContour = pitchContour.Select((p, i) =>
            {
                int windowSize = 3;
                int halfWindow = windowSize / 2;
                int start = Math.Max(0, i - halfWindow);
                int end = Math.Min(pitchContour.Count - 1, i + halfWindow);
                return pitchContour.Skip(start).Take(end - start + 1).Average();
            }).ToList();

            double averagePitch = pitchContour.Average();
            double pitchStdDev = Math.Sqrt(pitchContour.Average(v => Math.Pow(v - averagePitch, 2)));

            _logger.LogInformation($"Средняя частота основного тона: {averagePitch} Гц");
            _logger.LogInformation($"Флуктуации частоты основного тона (СКО): {pitchStdDev} Гц");

            ResponseData.Pitche = averagePitch;
            ResponseData.Fluctuation = pitchStdDev;
        }

        // для всей полосы
        private async Task ExtractGlobalFormants()
        {
            int samplingRate = DiscreteSignal.SamplingRate;
            int lpcOrder = LpcOrder;

            var samples = DiscreteSignal.Samples;
            var energy = samples.Sum(sample => sample * sample);

            if (energy < EnergyThreshold)
            {
                _logger.LogInformation("Недостаточная энергия сигнала для анализа.");
                return;
            }

            _logger.LogInformation($"Энергия: {energy}");
            var windowedSamples = samples.Zip(NWaves.Windows.Window.OfType(NWaves.Windows.WindowType.Hamming, samples.Length), (s, w) => s * w).ToArray();

            var autocorr = new float[lpcOrder + 1];
            for (int j = 0; j <= lpcOrder; j++)
            {
                autocorr[j] = windowedSamples.Take(samples.Length - j).Zip(windowedSamples.Skip(j), (a, b) => a * b).Sum();
            }

            var lpcCoefficients = new float[lpcOrder + 1];
            var error = Filter.LevinsonDurbin(autocorr, lpcCoefficients, lpcOrder);

            var roots = MathUtils.PolynomialRoots(lpcCoefficients.Select(c => (double)c).ToArray());
            var formants = roots
                .Where(r => r.Imaginary >= 0)
                .Select(r => (float)(samplingRate * Math.Atan2(r.Imaginary, r.Real) / (2 * Math.PI)))
                .OrderBy(f => f)
                .Take(3)
                .ToArray();

            _logger.LogInformation("Средние формантные частоты для всего сигнала:");
            for (int j = 0; j < formants.Length; j++)
            {
                _logger.LogInformation($"F{j + 1}: {formants[j]} Гц");
            }

            ResponseData.Formants = formants.Select(s => (double)s).ToArray();
        }



        // для кусков
        private async Task ExtractFormants()
        {
            int samplingRate = DiscreteSignal.SamplingRate;
            int frameSize = FrameSize;
            int hopSize = HopSize;
            int lpcOrder = LpcOrder;

            for (int i = 0; i <= DiscreteSignal.Length - frameSize; i += hopSize)
            {
                var frameSamples = DiscreteSignal.Samples.Skip(i).Take(frameSize).ToArray();
                var energy = frameSamples.Sum(sample => sample * sample);

                if (energy < EnergyThreshold)
                {
                    continue;
                }
                _logger.LogInformation($"Энергия: {energy}");

                var windowedFrame = frameSamples.Zip(NWaves.Windows.Window.OfType(NWaves.Windows.WindowType.Hamming, frameSize), (s, w) => s * w).ToArray();
                var autocorr = new float[lpcOrder + 1];
                for (int j = 0; j <= lpcOrder; j++)
                {
                    autocorr[j] = windowedFrame.Take(frameSize - j).Zip(windowedFrame.Skip(j), (a, b) => a * b).Sum();
                }

                var lpcCoefficients = new float[lpcOrder + 1];
                var error = Filter.LevinsonDurbin(autocorr, lpcCoefficients, lpcOrder);

                var roots = MathUtils.PolynomialRoots(lpcCoefficients.Select(c => (double)c).ToArray());
                var formants = roots
                    .Where(r => r.Imaginary >= 0)
                    .Select(r => (float)(samplingRate * Math.Atan2(r.Imaginary, r.Real) / (2 * Math.PI)))
                    .OrderBy(f => f)
                    .Take(3)
                    .ToArray();

                _logger.LogInformation("Формантные частоты:");
                for (int j = 0; j < formants.Length; j++)
                {
                    _logger.LogInformation($"F{j + 1}: {formants[j]} Гц");
                }
            }
        }

        private async Task AnalyzeRhythm()
        {
            int samplingRate = DiscreteSignal.SamplingRate;
            int signalLength = DiscreteSignal.Length;
            int frameIndex = 0;
            bool isSpeech = false;
            int speechStart = 0;
            int frameSize = FrameSize;
            int hopSize = HopSize;

            float maxAmplitude = DiscreteSignal.Samples.Max(Math.Abs);
            if (maxAmplitude > 0)
            {
                for (int i = 0; i < DiscreteSignal.Length; i++)
                {
                    DiscreteSignal.Samples[i] /= maxAmplitude;
                }
            }

            List<float> energyList = new List<float>();
            for (int i = 0; i < signalLength; i += hopSize)
            {
                int frameEnd = Math.Min(i + frameSize, signalLength);
                float[] frameSamples = DiscreteSignal.Samples.Skip(i).Take(frameEnd - i).ToArray();
                var frame = new DiscreteSignal(samplingRate, frameSamples);
                energyList.Add(frame.Energy());
            }

            int smoothingWindow = 5;
            List<float> smoothedEnergies = new List<float>();
            for (int j = 0; j < energyList.Count; j++)
            {
                var window = energyList.Skip(Math.Max(0, j - smoothingWindow / 2)).Take(smoothingWindow);
                smoothedEnergies.Add(window.Average());
            }

            double dynamicThreshold = smoothedEnergies.Average() * 0.5;
            List<double> duractionWords = new();

            foreach (var energy in smoothedEnergies)
            {
                _logger.LogInformation($"Энергия фрейма {frameIndex}: {energy}");

                if (energy > dynamicThreshold)
                {
                    if (!isSpeech)
                    {
                        isSpeech = true;
                        speechStart = frameIndex;
                    }
                }
                else
                {
                    if (isSpeech)
                    {
                        isSpeech = false;
                        int speechEnd = frameIndex;
                        double duration = (speechEnd - speechStart) * hopSize / (double)samplingRate;
                        duractionWords.Add(duration);
                        _logger.LogInformation($"Длительность слова: {duration} с");
                    }
                }
                frameIndex++;
            }

            ResponseData.DurationWords = duractionWords.ToArray();
        }

        private async Task ExtractCepstrum()
        {
            var fft = new Fft(FrameSize);

            var window = NWaves.Windows.Window.OfType(NWaves.Windows.WindowType.Hamming, FrameSize);
            var samples = DiscreteSignal.Samples.Take(FrameSize).ToArray();
            samples.ApplyWindow(window);

            var re = new float[FrameSize];
            var im = new float[FrameSize];

            Array.Copy(samples, re, samples.Length);
            fft.Direct(re, im);

            var magnitude = re.Zip(im, (r, i) => (float)Math.Sqrt(r * r + i * i)).ToArray();
            var logSpectrum = magnitude.Select(m => (float)Math.Log(m + 1e-10)).ToArray();

            Array.Clear(im, 0, im.Length);
            fft.Inverse(logSpectrum, im);

            int numCoeffs = 13;
            _logger.LogInformation("Кепстральные коэффициенты:");
            for (int i = 0; i < numCoeffs; i++)
            {
                _logger.LogInformation($"[Коэф. №{i}] {logSpectrum[i]}");
            }
        }

        private async Task ComputeAverageSlope()
        {
            int N = Spectrum.Length;
            if (N == 0)
            {
                _logger.LogWarning("Длина спектра нулевая, невозможно вычислить наклон.");
                return;
            }

            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumXX = 0;

            for (int i = 0; i < N; i++)
            {
                double freq = i;
                double spectrumValue = Spectrum[i];

                if (spectrumValue <= 0)
                {
                    continue;
                }

                double magnitude = 20 * Math.Log10(spectrumValue);

                sumX += freq;
                sumY += magnitude;
                sumXY += freq * magnitude;
                sumXX += freq * freq;
            }

            double denominator = N * sumXX - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10)
            {
                _logger.LogWarning("Знаменатель слишком близок к нулю, невозможно вычислить наклон.");
                return;
            }

            double slope = (N * sumXY - sumX * sumY) / denominator;
            _logger.LogInformation($"Средний наклон спектра: {slope}");
        }



        private DiscreteSignal PreprocessSignal(DiscreteSignal signal)
        {
            float maxAmplitude = signal.Samples.Max(Math.Abs);
            if (maxAmplitude > 0)
            {
                for (int i = 0; i < signal.Length; i++)
                {
                    signal.Samples[i] /= maxAmplitude;
                }
            }

            var highPassFilter = new NWaves.Filters.BiQuad.HighPassFilter(100, signal.SamplingRate);
            signal = highPassFilter.ApplyTo(signal);

            var window = NWaves.Windows.Window.OfType(NWaves.Windows.WindowType.Hamming, FrameSize);

            for (int i = 0; i < signal.Length; i += FrameSize)
            {
                var frameSamples = signal.Samples.Skip(i).Take(FrameSize).ToArray();
                for (int j = 0; j < frameSamples.Length; j++)
                {
                    signal.Samples[i + j] = frameSamples[j] * window[j];
                }
            }
            return signal;
        }


        public void ExtractSignalLevel(DiscreteSignal signal)
        {
            var rms = signal.Rms();
            var levelDb = 20 * Math.Log10(rms + 1e-10);
            Console.WriteLine($"Уровень сигнала: {levelDb} дБ");
        }


        private async Task<byte[]> GetVoiceByteArrayAsync(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream);
                var audioBytes = memoryStream.ToArray();
                return audioBytes;
            }
        }

        private async Task Draw(PlotType type)
        {
            try
            {
                await Drawer.DrawPlot(type, SampleRate, Spectrum);
                _logger.LogInformation($"[{nameof(Draw)}] Графики были успешно нарисованы");
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex ,$"[{nameof(Draw)}] Ошибка при создании графиков");
            }
        }


        private bool _disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (AudioBytes != null)
                    {
                        AudioBytes = null;
                    }
                    if (Spectrum != null)
                    {
                        Spectrum = null;
                    }
                    _disposed = true;
                }
            }
        }

        ~VoiceManager()
        {
            Dispose(false);
        }
    }

}
