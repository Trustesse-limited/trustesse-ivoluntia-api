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
    }
}
