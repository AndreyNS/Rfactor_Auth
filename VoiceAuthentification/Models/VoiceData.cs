using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace VoiceAuthentification.Models
{
    public class VoiceDataRaw
    {
        public string Phrase { get; set; } // сама фраза
        public double Pitche { get; set; }  // средняя частота основного тона
        public double Fluctuation { get; set; } // средняя флуктуация
        public double[] Formants { get; set; } // форманты на всей дорожке
        public double[] DurationWords { get; set; } // длительность слов

        public string ToEncryptedString(string encryptionKey)
        {
            var jsonData = JsonConvert.SerializeObject(this);
            return EncryptString(jsonData, encryptionKey);
        }

        public static VoiceDataRaw FromEncryptedString(string encryptedData, string encryptionKey)
        {
            var jsonData = DecryptString(encryptedData, encryptionKey);
            return JsonConvert.DeserializeObject<VoiceDataRaw>(jsonData);
        }

        private static string EncryptString(string text, string key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = new byte[16];
                var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (var ms = new System.IO.MemoryStream())
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new System.IO.StreamWriter(cs))
                        sw.Write(text);
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string DecryptString(string cipherText, string key)
        {
            var buffer = Convert.FromBase64String(cipherText);
            using (var aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key.PadRight(32));
                aes.IV = new byte[16];
                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using (var ms = new System.IO.MemoryStream(buffer))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var sr = new System.IO.StreamReader(cs))
                    return sr.ReadToEnd();
            }
        }
    }
}
