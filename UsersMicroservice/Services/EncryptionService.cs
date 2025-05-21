using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using UsersMicroservice.Configurations;

namespace UsersMicroservice.Services
{
    public class EncryptionService
    {
        private readonly string key;
        private readonly string iv;

        public EncryptionService(IOptions<EncryptionSettings> options)
        {
            var encryptionSettings = options.Value;

            key = encryptionSettings.AESKey;
            iv = encryptionSettings.AESIV;

            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv))
            {
                throw new ArgumentNullException("Encryption keys cannot be null or empty");
            }
        }

        public string Encrypt(string text)
        {
            using var aes = Aes.Create();

            aes.KeySize = 128; // Explicitly set to 128-bit key size (16 characters)
            aes.BlockSize = 128; // AES block size is always 128 (16 characters)

            // Load Key and IV from the instance fields
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                using var sw = new StreamWriter(cs, Encoding.UTF8);
                sw.Write(text);
                sw.Close();
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string cipherText)
        {
            using var aes = Aes.Create();

            aes.KeySize = 128; // Explicitly set to 128-bit key size (16 characters)
            aes.BlockSize = 128; // AES block size is always 128 (16 characters)

            // Load Key and IV from the instance fields
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = Encoding.UTF8.GetBytes(iv);

            // Convert the cipher text from Base64
            var buffer = Convert.FromBase64String(cipherText);

            // Create the decryptor and memory streams
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs, Encoding.UTF8);

            // Read the decrypted text
            string decryptedText = sr.ReadToEnd();
            return decryptedText;
        }
    }
}
