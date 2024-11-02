namespace VoiceAuthentification.AudioHandlers
{
    public static class Filter
    {
        public static double[] noisySine = new double[20] { 40, 41, 38, 40, 45, 42, 43, 44, 40, 38, 44, 45, 40, 39, 37, 41, 42, 70, 44, 42 };
        public static double[] clean = new double[20];
         
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
    }
}
