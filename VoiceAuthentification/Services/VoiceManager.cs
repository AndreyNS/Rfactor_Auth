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

namespace VoiceAuthentification.Services
{
    public class VoiceManager : IDisposable
    {
        private const int SampleRate = 44100;
        private byte[] AudioBytes { get; set; }
        private float[] Spectrum { get; set; }
        public string ResponseData { get; private set; }

        public async Task SetStreamAudio(Stream stream) => AudioBytes = await GetVoiceByteArrayAsync(stream);
        public async Task VoiceProcessAsync()
        {
            try
            {
                Spectrum = await AnalyzeAsync(AudioBytes);

            }
            catch (Exception ex)
            {
                return;
            }
        }

        private async Task Draw()
        {
            await Drawer.DrawPlot(PlotType.All, SampleRate, Spectrum);
        }

        private async Task<float[]> AnalyzeAsync(byte[] audioBytes)
        {
            int floatCount = audioBytes.Length / sizeof(float);
            float[] samples = new float[floatCount];

            for (int i = 0; i < floatCount; i++)
            {
                samples[i] = BitConverter.ToSingle(audioBytes, i * sizeof(float));
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
            return spectrum;
        }

        // Спектрограмма
        private async Task PlotSpectrogramAsync(float[] samples, int sampleRate)
        {
            try
            {
                int fftSize = 1024;
                int hopSize = 512;
                var stft = new Stft(fftSize, hopSize, WindowType.Hamming);
                var magnitudeSpectrogram = stft.Spectrogram(samples);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // АЧХ
        private async Task PlotFrequencyResponseAsync(float[] samples, int sampleRate)
        {
            try
            {
                int fftSize = 1024;
                float[] paddedSamples = new float[fftSize];
                Array.Copy(samples, paddedSamples, Math.Min(samples.Length, fftSize));

                var fft = new RealFft(fftSize);
                var spectrum = new float[fftSize / 2 + 1];
                fft.MagnitudeSpectrum(paddedSamples, spectrum);

                var frequencies = Enumerable.Range(0, fftSize / 2 + 1)
                                            .Select(i => i * sampleRate / (double)fftSize)
                                            .ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        //private void ExtractPitch(DiscreteSignal signal)
        //{
        //    int samplingRate = signal.SamplingRate;
        //    int frameSize = 1024; // Размер окна анализа (может быть настроен)
        //    int hopSize = 512;    // Шаг окна

        //    var pitchExtractor = new PitchExtractor(new NWaves.FeatureExtractors.Options.PitchOptions
        //    { samplingRate, frameSize, hopSize, PitchDetectorMethod.Autocorrelation
        //    });

        //    var pitches = pitchExtractor.CollectFeatures(signal);

        //    // pitches - список массивов, каждый из которых содержит один элемент (частоту основного тона)
        //    foreach (var pitchArray in pitches)
        //    {
        //        float pitch = pitchArray[0]; // Частота основного тона в Гц
        //        Console.WriteLine($"Частота основного тона: {pitch} Гц");
        //    }

        //    List<float> pitchContour = new List<float>();

        //    foreach (var pitchArray in pitches)
        //    {
        //        float pitch = pitchArray[0];
        //        pitchContour.Add(pitch);
        //    }

        //    // Теперь pitchContour содержит интонационный контур, который можно анализировать или визуализировать

        //    double averagePitch = pitchContour.Average();
        //    double pitchStdDev = Math.Sqrt(pitchContour.Average(v => Math.Pow(v - averagePitch, 2)));

        //    Console.WriteLine($"Средняя частота основного тона: {averagePitch} Гц");
        //    Console.WriteLine($"Флуктуации частоты основного тона (СКО): {pitchStdDev} Гц");

        //}



        //// Извлечение формантных частот
        //public void ExtractFormants(DiscreteSignal signal)
        //{
        //    int samplingRate = signal.SamplingRate;
        //    int frameSize = 1024;
        //    int hopSize = 512;
        //    int lpcOrder = 12; // Порядок LPC (можно настроить)

        //    var formantExtractor = new FormantExtractor(samplingRate, frameSize, hopSize, lpcOrder);

        //    var formantTracks = formantExtractor.CollectFeatures(signal);

        //    // formantTracks - список массивов с формантными частотами для каждого фрейма
        //    foreach (var formants in formantTracks)
        //    {
        //        Console.WriteLine("Формантные частоты:");
        //        for (int i = 0; i < formants.Length; i++)
        //        {
        //            Console.WriteLine($"F{i + 1}: {formants[i]} Гц");
        //        }
        //    }

        //    List<float[]> formantContours = new List<float[]>();

        //    foreach (var formants in formantTracks)
        //    {
        //        formantContours.Add(formants);
        //    }

        //    // Анализируем изменения формант во времени
        //    for (int i = 1; i < formantContours.Count; i++)
        //    {
        //        for (int j = 0; j < formantContours[i].Length; j++)
        //        {
        //            float delta = formantContours[i][j] - formantContours[i - 1][j];
        //            Console.WriteLine($"Изменение F{j + 1} между фреймами {i - 1} и {i}: {delta} Гц");
        //        }
        //    }
        //}

        // Вычисление огибающей спектра и его среднего наклона
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

        //private double ComputeAverageSlope(double[] spectrum)
        //{
        //    // Метод для вычисления среднего наклона спектра
        //    // Можно использовать линейную регрессию на логарифмических значениях частоты и амплитуды
        //    int N = spectrum.Length;
        //    double sumX = 0;
        //    double sumY = 0;
        //    double sumXY = 0;
        //    double sumXX = 0;

        //    for (int i = 0; i < N; i++)
        //    {
        //        double freq = i;
        //        double magnitude = 20 * Math.Log10(spectrum[i] + 1e-10); // В дБ

        //        sumX += freq;
        //        sumY += magnitude;
        //        sumXY += freq * magnitude;
        //        sumXX += freq * freq;
        //    }

        //    double slope = (N * sumXY - sumX * sumY) / (N * sumXX - sumX * sumX);
        //    return slope;
        //}

        // кепстральные коэф
        //public void ExtractCepstrum(DiscreteSignal signal)
        //{
        //    int frameSize = 1024;
        //    var fft = new Fft(frameSize);

        //    var window = NWaves.Windows.Window.OfType(NWaves.Windows.WindowTypes.Hamming, frameSize);
        //    var samples = signal.Samples.Take(frameSize).ToArray();
        //    samples.ApplyWindow(window);

        //    var re = new float[frameSize];
        //    var im = new float[frameSize];

        //    fft.Direct(samples, re, im);

        //    // логарифм амплитудного спектра
        //    var magnitude = re.Zip(im, (r, i) => (float)Math.Sqrt(r * r + i * i)).ToArray();
        //    var logSpectrum = magnitude.Select(m => (float)Math.Log(m + 1e-10)).ToArray();

        //    // Обратное преобразование Фурье
        //    var cepstrum = new float[frameSize];
        //    fft.Inverse(logSpectrum, cepstrum);

        //    // первые N кепстральных коэффициентов
        //    int numCoeffs = 13;
        //    Console.WriteLine("Кепстральные коэффициенты:");
        //    for (int i = 0; i < numCoeffs; i++)
        //    {
        //        Console.WriteLine(cepstrum[i]);
        //    }
        //}

        // Анализ ритма и длительности слов
        //public void AnalyzeRhythm(DiscreteSignal signal)
        //{
        //    int frameSize = 1024;
        //    int hopSize = 512;
        //    double energyThreshold = 0.01;

        //    var frames = signal.SliceFrames(frameSize, hopSize);
        //    bool isSpeech = false;
        //    int speechStart = 0;
        //    int frameIndex = 0;

        //    foreach (var frame in frames)
        //    {
        //        var energy = frame.Energy();

        //        if (energy > energyThreshold)
        //        {
        //            if (!isSpeech)
        //            {
        //                isSpeech = true;
        //                speechStart = frameIndex;
        //            }
        //        }
        //        else
        //        {
        //            if (isSpeech)
        //            {
        //                isSpeech = false;
        //                int speechEnd = frameIndex;
        //                double duration = (speechEnd - speechStart) * hopSize / (double)signal.SamplingRate;
        //                Console.WriteLine($"Длительность слова: {duration} с");
        //            }
        //        }

        //        frameIndex++;
        //    }
        //}

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
