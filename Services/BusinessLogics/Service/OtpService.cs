using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        public OtpService(IOtpRepository otpRepository, UserManager<User> userManager)
        {
            _otpRepository = otpRepository;
            _userManager = userManager;
        }

        public async Task<bool> ConfirmOtpAsync(string userId, string otpCode, OtpPurpose purpose)
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

        public async Task<string> GenerateOtpAsync(string userId, OtpPurpose purpose)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            string otpCode = OtpUtility.GenerateRandomCode(6, true);

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
    }
}
