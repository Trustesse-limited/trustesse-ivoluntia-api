using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.uitilities;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly UserManager<User> _userManager;  
        private readonly IUnitOfWork _uow;
        public OtpService(IOtpRepository otpRepository, UserManager<User> userManager, IUnitOfWork uow)
        {
            _otpRepository = otpRepository;
            _userManager = userManager;
            _uow = uow;
        }

        public async Task<ApiResponse<Otp>> ConfirmOtpAsync(string otpCode, string otpPurpose)
        {
            var otp = await _otpRepository.GetOtpByCodeAsync(otpCode, otpPurpose);
            if (otp == null)
                return ApiResponse<Otp>.Failure(StatusCodes.Status404NotFound, "otp not found") ;

            if (otp.IsUsed)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "otp already used");

            if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "already used"); ;
            await _otpRepository.MarkOtpAsUsedAsync(otp);
            return ApiResponse<Otp>.Success("success", otp); 
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
