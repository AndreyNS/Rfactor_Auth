using NWaves.Transforms;
using NWaves.Windows;
using System.Drawing;
using System.Drawing.Imaging;

namespace VoiceAuthentification.AudioHandlers
{
    public static class Drawer
    {
        private const string FormatType = ".png";
        private const string PlotFolder = "PlotSignal";
        private const string Spectrum = $"spectrum{FormatType}";
        private const string Spectrogram = $"spectrogram{FormatType}";
        private const string FrequencyResponse = $"frequency_response{FormatType}";


        public static async Task DrawPlot(PlotType plotType, int sampleRate, params float[] values)
        {
            switch (plotType)
            {
                case PlotType.None:
                    {
                        return;
                    }
                case PlotType.Spectrum:
                    {
                        await SpectrumPlotAsync(values);
                        return;
                    }
                case PlotType.Spectrogram:
                    {
                        await SpectrogramPlotAsync(values, sampleRate);
                        return;
                    }
                case PlotType.FrequencyResponse:
                    {
                        await FrequencyResponsePlotAsync(values, sampleRate);
                        return;
                    }
                case PlotType.All:
                    {
                        await SpectrumPlotAsync(values);
                        await SpectrogramPlotAsync(values, sampleRate);
                        await FrequencyResponsePlotAsync(values, sampleRate);
                        return;
                    }
            }
        }

        private static async Task SpectrumPlotAsync(float[] spectrum)
        {
            try
            {
                var bitmap = new Bitmap(600, 400);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                    var pen = new Pen(Color.Red);

                    float xScale = 600f / spectrum.Length;
                    float yScale = 400f / spectrum.Max();

                    for (int i = 0; i < spectrum.Length - 1; i++)
                    {
                        float x1 = i * xScale;
                        float y1 = 400 - (spectrum[i] * yScale);
                        float x2 = (i + 1) * xScale;
                        float y2 = 400 - (spectrum[i + 1] * yScale);

                        if (float.IsFinite(y1) && float.IsFinite(y2))
                        {
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                    }
                }
                await PlotSaverAsync(Spectrum, bitmap);
            }
            catch (Exception ex)
            {

            }
        }

        private static async Task SpectrogramPlotAsync(float[] samples, int sampleRate)
        {
            try
            {
                int fftSize = 1024;
                int hopSize = 512;
                var stft = new Stft(fftSize, hopSize, WindowType.Hamming);
                var magnitudeSpectrogram = stft.Spectrogram(samples);

                var bitmap = new Bitmap(600, 400);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);

                    float xScale = 600f / magnitudeSpectrogram.Count();
                    float yScale = 400f / (sampleRate / 2);

                    float maxMagnitude = magnitudeSpectrogram.Max(row => row.Max());

                    for (int i = 0; i < magnitudeSpectrogram.Count(); i++)
                    {
                        for (int j = 0; j < magnitudeSpectrogram[i].Length; j++)
                        {
                            float x = i * xScale;
                            float y = 400 - (j * yScale);
                            int intensity = (int)(255 * (magnitudeSpectrogram[i][j] / maxMagnitude));
                            var color = Color.FromArgb(intensity, 0, 0);

                            g.FillRectangle(new SolidBrush(color), x, y, xScale, yScale);
                        }
                    }
                }
                await PlotSaverAsync(Spectrogram, bitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        private static async Task FrequencyResponsePlotAsync(float[] samples, int sampleRate)
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

                var bitmap = new Bitmap(600, 400);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.White);
                    var pen = new Pen(Color.Blue);

                    float xScale = 600f / frequencies.Length;
                    float yScale = 400f / spectrum.Max();

                    for (int i = 0; i < spectrum.Length - 1; i++)
                    {
                        float x1 = i * xScale;
                        float y1 = 400 - (spectrum[i] * yScale);
                        float x2 = (i + 1) * xScale;
                        float y2 = 400 - (spectrum[i + 1] * yScale);

                        if (float.IsFinite(y1) && float.IsFinite(y2))
                        {
                            g.DrawLine(pen, x1, y1, x2, y2);
                        }
                    }
                }
                await PlotSaverAsync(FrequencyResponse, bitmap);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static async Task PlotSaverAsync(string namePlot, Bitmap bitmap)
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), PlotFolder);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            path = Path.Combine(path, namePlot);
            bitmap.Save(path, ImageFormat.Png);
        }
    }

    public enum PlotType
    {
        All, // Все
        Spectrum, // Спектр
        Spectrogram, // Зависимость частоты от времени
        FrequencyResponse, // АЧХ

        None
    }
}
