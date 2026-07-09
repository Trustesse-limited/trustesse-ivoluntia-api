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
        private readonly IUnitOfWork _uow;
        public OtpService(IOtpRepository otpRepository, UserManager<User> userManager, IUnitOfWork uow)
        {
            _otpRepository = otpRepository;
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<bool> ConfirmOtpAsync(string userId, string otpCode, OtpPurpose purpose)
        {
            var otp = await _uow.otpRepo.GetByExpressionAsync(x => x.UserId == userId && x.OtpCode == otpCode && x.Purpose == purpose.ToString());

            if (otp == null)
                return false;

            if (otp.IsUsed)
                return false;

            if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
                return false;

            otp.IsUsed = true;

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
                Purpose = purpose.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false
            };

            await _uow.otpRepo.AddAsync(otp);
            await _uow.CompleteAsync();
            return otpCode;
        }
    }
}
