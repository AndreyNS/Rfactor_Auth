using VoiceAuthentification.AudioHandlers;
using PdfSharp.Drawing;
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

namespace VoiceAuthentification.Services
{
    public class VoiceManager : IVoiceManager, IDisposable
    {
        private const int SampleRate = 22050;
        private const int FrameSize = 256;
        private const int HopSize = 128;
        private const double EnergyThreshold = 64;

        private readonly ILogger<VoiceManager> _logger;
        private readonly SpeechRecognition _speechRecognition;

        private byte[] AudioBytes { get; set; }
        private float[] Spectrum { get; set; }
        public string ResponseData { get; private set; }

        private DiscreteSignal DiscreteSignal { get; set; }
        
        public VoiceManager(ILogger<VoiceManager> logger)
        {
            _logger = logger;
            _speechRecognition = new();
        }
        public async Task SetStreamAudio(Stream stream) => AudioBytes = await GetVoiceByteArrayAsync(stream);
        public async Task VoiceProcessAsync()
        {
            try
            {
                await _speechRecognition.RecognizeSpeechFromBytes(AudioBytes);

                await AnalyzeAsync();
                DiscreteSignal = PreprocessSignal(DiscreteSignal);

                await ExtractPitch(); // Частота основного тона
                //await ExtractFormants(); // Извлечение формантных частот
                //await AnalyzeRhythm(); // Анализ длительности слова и ритма
                //await ExtractCepstrum(); // Кэпстральные коэф (нестабильно)
                //await ComputeAverageSlope(); Средний наклон спектра (нестабильно)
            }
            catch (Exception ex)
            {
                return;
            }
        }
  
        private async Task AnalyzeAsync()
        {
            int floatCount = AudioBytes.Length / sizeof(float);
            float[] samples = new float[floatCount];

            for (int i = 0; i < floatCount; i++)
            {
                samples[i] = BitConverter.ToSingle(AudioBytes, i * sizeof(float));
                if (float.IsNaN(samples[i]) || float.IsInfinity(samples[i]) || Math.Abs(samples[i]) > 1e10)
                {
                    samples[i] = 0;
                }
            }

            var window = NWaves.Windows.Window.OfType(NWaves.Windows.WindowType.Hamming, samples.Length);
            var windowedSamples = samples.Zip(window, (s, w) => s * w).ToArray();

            int fftSize = 1024;
            var fft = new RealFft(fftSize);
            var spectrum = new float[fftSize / 2 + 1];

            fft.MagnitudeSpectrum(windowedSamples, spectrum);

            if (spectrum.Any(x => float.IsNaN(x)))
            {
                throw new Exception("Спектр содержит NaN значения.");
            }

            DiscreteSignal = new DiscreteSignal(SampleRate, spectrum);
            Spectrum = spectrum;
        }
        private async Task ExtractPitch()
        {
            int samplingRate = DiscreteSignal.SamplingRate;

            var pitchExtractor = new PitchExtractor(new NWaves.FeatureExtractors.Options.PitchOptions
            {
                SamplingRate = samplingRate,
                FrameSize = FrameSize,
                HopSize = HopSize,
            });

            var pitches = pitchExtractor.ComputeFrom(DiscreteSignal);
            foreach (var pitchArray in pitches)
            {
                float pitch = pitchArray[0];
                _logger.LogInformation($"Частота основного тона: {pitch} Гц");
            }

            List<float> pitchContour = new List<float>();

            foreach (var pitchArray in pitches)
            {
                float pitch = pitchArray[0];
                pitchContour.Add(pitch);
            }

            double averagePitch = pitchContour.Average();
            double pitchStdDev = Math.Sqrt(pitchContour.Average(v => Math.Pow(v - averagePitch, 2)));

            _logger.LogInformation($"Средняя частота основного тона: {averagePitch} Гц");
            _logger.LogInformation($"Флуктуации частоты основного тона (СКО): {pitchStdDev} Гц");

        }
        private async Task ExtractFormants()
        {
            int samplingRate = DiscreteSignal.SamplingRate;
            int frameSize = 256;
            int hopSize = 128;
            int lpcOrder = 10; 

            for (int i = 0; i <= DiscreteSignal.Length - frameSize; i += hopSize)
            {
                var frameSamples = DiscreteSignal.Samples.Skip(i).Take(frameSize).ToArray();
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
            int frameSize = 512;
            int hopSize = 256;


            float maxAmplitude = DiscreteSignal.Samples.Max(Math.Abs);
            if (maxAmplitude > 0)
            {
                for (int i = 0; i < DiscreteSignal.Length; i++)
                {
                    DiscreteSignal.Samples[i] /= maxAmplitude;
                }
            }


            for (int i = 0; i < signalLength; i += hopSize)
            {
                int frameEnd = Math.Min(i + frameSize, signalLength);
                float[] frameSamples = DiscreteSignal.Samples.Skip(i).Take(frameEnd - i).ToArray();
                var frame = new DiscreteSignal(samplingRate, frameSamples);

                var energy = frame.Energy();

                // Временный отладочный вывод энергии
                _logger.LogInformation($"Энергия фрейма {frameIndex}: {energy}");

                if (energy > EnergyThreshold)
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
                        _logger.LogInformation($"Длительность слова: {duration} с");
                    }
                }
                frameIndex++;
            }
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
                _logger.LogInformation($"{logSpectrum[i]}");
            }
        }

        //// Вычисление огибающей спектра и его среднего наклона
        //public void ExtractSpectralEnvelope(DiscreteSignal signal)
        //{
        //    int order = 12; // Порядок LPC

        //    var lpc = new LpcAnalyzer(order);
        //    var lpcCoeffs = lpc.Analyze(signal);

        //    // Создаем LPC фильтр
        //    var lpcFilter = new TransferFunction(new[] { 1.0 }, lpcCoeffs);

        //    // Получаем частотную характеристику
        //    var freqResponse = lpcFilter.FrequencyResponse(512);

        //    // freqResponse содержит комплексные значения частотной характеристики
        //    // Можно вычислить амплитудный спектр (модуль частотной характеристики)
        //    var magnitudeSpectrum = freqResponse.Magnitude;

        //    // Вычисляем средний наклон спектра
        //    double averageSlope = ComputeAverageSlope(magnitudeSpectrum);
        //    Console.WriteLine($"Средний наклон спектра: {averageSlope}");
        //}

        private async Task ComputeAverageSlope()
        {
            // Метод для вычисления среднего наклона спектра
            // Можно использовать линейную регрессию на логарифмических значениях частоты и амплитуды
            int N = Spectrum.Length;
            double sumX = 0;
            double sumY = 0;
            double sumXY = 0;
            double sumXX = 0;

            for (int i = 0; i < N; i++)
            {
                double freq = i;
                double magnitude = 20 * Math.Log10(Spectrum[i] + 1e-10); // В дБ

                sumX += freq;
                sumY += magnitude;
                sumXY += freq * magnitude;
                sumXX += freq * freq;
            }

            double slope = (N * sumXY - sumX * sumY) / (N * sumXX - sumX * sumX);
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


        // Извлечение уровня сигнала
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

        private async Task Draw()
        {
            await Drawer.DrawPlot(PlotType.All, SampleRate, Spectrum);
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
