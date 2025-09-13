namespace Trustesse.Ivoluntia.Commons.uitilities
{
    public static class OtpUtility
    {
        public static string GenerateRandomCode(int size, bool includeAlphabet, string defaultValue = "")
        {
            if (!string.IsNullOrEmpty(defaultValue)) return defaultValue;

            var chars = includeAlphabet ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789" : "0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, size)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
