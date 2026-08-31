using System.Security.Cryptography;
using System.Text;

namespace Trustesse.Ivoluntia.Commons.Cryptography
{
    public static class AES
    {
        public static string Decrypt(string encryptedData, string key, string iv)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv)) return string.Empty;
                var IV = Encoding.UTF8.GetBytes(iv);
                byte[] input = Convert.FromBase64String(encryptedData);
                var cipher = Aes.Create();
                cipher.Padding = PaddingMode.PKCS7;
                cipher.Mode = CipherMode.CBC;
                cipher.IV = IV;
                cipher.Key = Encoding.UTF8.GetBytes(key);
                var DecryptText = cipher.CreateDecryptor(cipher.Key, cipher.IV);

                var newClearData = DecryptText.TransformFinalBlock(input, 0, input.Length);
                var finaltext = Encoding.UTF8.GetString(newClearData);
                return finaltext;
            }
            catch (Exception)
            {
                return string.Empty;
            }

        }
        public static string Encrypt(string cleartext, string key, string iv)
        {
            try
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(iv)) return string.Empty;
                var IV = Encoding.UTF8.GetBytes(iv);
                var cipher = Aes.Create();
                cipher.Padding = PaddingMode.PKCS7;
                cipher.Mode = CipherMode.CBC;
                cipher.IV = IV;
                cipher.Key = Encoding.UTF8.GetBytes(key);
                var passwordEnc = cipher.CreateEncryptor(cipher.Key, cipher.IV);
                var buffer = Encoding.UTF8.GetBytes(cleartext);
                var transformer = passwordEnc.TransformFinalBlock(buffer, 0, buffer.Length);
                return Convert.ToBase64String(transformer);
                
            }
            catch (Exception )
            {
                return string.Empty;
            }
        }

        public static string EncryptData(string plainText, byte[] key)
        {
            if (string.IsNullOrEmpty(plainText))
                throw new ArgumentException(
                    "Plain text cannot be empty.",
                    nameof(plainText));

            byte[] nonce = RandomNumberGenerator.GetBytes(12);
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];
            using var aes = new AesGcm(key, 16);
            aes.Encrypt(
                nonce,
                plaintextBytes,
                ciphertext,
                tag);
            var result = new byte[
                nonce.Length +
                tag.Length +
                ciphertext.Length];

            Buffer.BlockCopy(
                nonce,
                0,
                result,
                0,
                nonce.Length);

            Buffer.BlockCopy(
                tag,
                0,
                result,
                nonce.Length,
                tag.Length);

            Buffer.BlockCopy(
                ciphertext,
                0,
                result,
                nonce.Length + tag.Length,
                ciphertext.Length);

            return Convert.ToBase64String(result);
        }

        public static string DecryptData(string encryptedText, byte[] key)
        {
            
                if (string.IsNullOrWhiteSpace(encryptedText))
                    return null;

                byte[] encryptedData =
                    Convert.FromBase64String(encryptedText);
                const int nonceSize = 12;
                const int tagSize = 16;
                if (encryptedData.Length < nonceSize + tagSize)
                    return null;

                byte[] nonce = encryptedData[..nonceSize];

                byte[] tag = encryptedData[
                    nonceSize..(nonceSize + tagSize)];

                byte[] ciphertext = encryptedData[
                    (nonceSize + tagSize)..];

                byte[] plaintext = new byte[ciphertext.Length];

                using var aes = new AesGcm(key, 16);

                aes.Decrypt(
                    nonce,
                    ciphertext,
                    tag,
                    plaintext);

                return Encoding.UTF8.GetString(plaintext); 
        }
    }
}
