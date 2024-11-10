using System.Security.Cryptography;
using System.Text;

namespace VoiceAuthentification.Services
{
    public class EncryptionService
    {
        private readonly IConfiguration _configuration;

        public EncryptionService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetDecryptedKey()
        {
            var encryptedKey = _configuration["EncryptionSettings:EncryptedKey"];
            var key = _configuration["EncryptionSettings:Key"];
            var iv = _configuration["EncryptionSettings:IV"];

            return encryptedKey;
        }

        private string DecryptKey(string encryptedKey, string _key, string _iv)
        {
            byte[] key = Convert.FromBase64String(_key);
            byte[] iv = Convert.FromBase64String(_iv);


            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                aes.Padding = PaddingMode.PKCS7;
                aes.Mode = CipherMode.CBC;

                var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] cipherTextBytes = Convert.FromBase64String(encryptedKey);

                using (var ms = new System.IO.MemoryStream(cipherTextBytes))
                using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (var reader = new System.IO.StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
