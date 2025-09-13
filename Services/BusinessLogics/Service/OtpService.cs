using Trustesse.Ivoluntia.Commons.uitilities;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class OtpService : IOtpService
    {
        readonly IOtpRepository _otpRepository;
        public OtpService(IOtpRepository otpRepository)
        {
            _otpRepository = otpRepository;
        }

        public async Task<bool> ConfirmOtpAsync(string userId, string otpCode, PurposeEnum purpose)
        {
            var code = await _otpRepository.GetOtpByCodeAsync(userId, otpCode, purpose);
            if (code == null)
                return false;

            if (code.IsUsed)
                return false;

            if((DateTime.UtcNow - code.CreatedAt).TotalMinutes > 5)
                return false;

            await _otpRepository.MarkOtpAsUsedAsync(code);

            return true;
        }

        public async Task<string> GenerateOtpAsync(string userId, PurposeEnum purpose)
        {
            string otpCode = OtpUtility.GenerateRandomCode(6, false);

            var otp = new Otp
            {
                UserId = userId,
                OtpCode = otpCode,
                Purpose = purpose,
                CreatedAt = DateTime.UtcNow,
                IsUsed = false
            };

            await _otpRepository.AddOtpAsync(otp);
            return otpCode;
        }

        //private string GenerateRandomCode(int size, bool includeAlphabet, string defaultValue = "")
        //{
        //    if (!string.IsNullOrEmpty(defaultValue))
        //        return defaultValue;

        //    var random = new Random();
        //    const string digits = "0123456789";
        //    const string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        //    string chars = digits + (includeAlphabet ? alphabets : "");

        //    return new string(Enumerable.Repeat(chars, size)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());
        //}
    }
}
