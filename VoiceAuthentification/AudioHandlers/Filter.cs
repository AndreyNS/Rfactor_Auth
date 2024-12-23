﻿using System.Numerics;

namespace VoiceAuthentification.AudioHandlers
{
    public static class Filter
    {
        private static double[] noisySine = new double[20] { 40, 41, 38, 40, 45, 42, 43, 44, 40, 38, 44, 45, 40, 39, 37, 41, 42, 70, 44, 42 };
        private static double[] clean = new double[20];
         
        public static void Kalman(double[] noisy)
        {
            double A = double.Parse("1");
            double H = double.Parse("1");
            double P = double.Parse("0.1");
            double Q = double.Parse("0.125");  
            double R = double.Parse("1"); 
            double K;
            double z;
            double x;

            x = noisy[0];
            for (int i = 0; i < noisy.Length; i++)
            {
                z = noisy[i];

                x = A * x;
                P = A * P * A + Q;

                K = P * H / (H * P * H + R);
                x = x + K * (z - H * x);
                P = (1 - K * H) * P;

                clean[i] = x;
                Console.WriteLine(noisy[i] + " " + clean[i]);
            }

            return;
        }

        public static double[] GetFrequencies(Complex[] fftResult, int sampleRate = 44100)
        {
            double[] magnitudes = fftResult.Select(complex => complex.Magnitude).ToArray();
            double[] frequencies = new double[fftResult.Length / 2];
            for (int i = 0; i < frequencies.Length; i++)
            {
                frequencies[i] = (double)i * sampleRate / fftResult.Length;
            }
            return frequencies;
        }

        public static float LevinsonDurbin(float[] autocorrelation, float[] lpc, int order)
        {
            var error = autocorrelation[0];
            for (int i = 1; i <= order; i++)
            {
                float lambda = -autocorrelation[i];
                for (int j = 1; j < i; j++)
                {
                    lambda -= lpc[j] * autocorrelation[i - j];
                }
                lambda /= error;

                for (int j = 1; j < i; j++)
                {
                    lpc[j] += lambda * lpc[i - j];
                }
                lpc[i] = lambda;
                error *= (1.0f - lambda * lambda);
            }
            return error;
        }
    }
}
