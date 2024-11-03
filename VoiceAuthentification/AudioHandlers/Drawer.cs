using NWaves.Transforms;
using NWaves.Windows;
using System.Drawing;
using System.Drawing.Imaging;

namespace VoiceAuthentification.AudioHandlers
{
    public static class Drawer
    {
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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "spectrum.png");
                bitmap.Save(filePath, ImageFormat.Png);
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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "spectrogram.png");
                bitmap.Save(filePath, ImageFormat.Png);
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
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "frequency_response.png");
                bitmap.Save(filePath, ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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
