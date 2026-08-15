using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.uitilities;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class OtpService : IOtpService
    {
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUnitOfWork _uow;
        public OtpService(UserManager<User> userManager, IUnitOfWork uow, ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _uow = uow;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<Otp>> ConfirmOtpAsync(string otpCode, string otpPurpose)
        {
            var otp = await _uow.OtpRepo.GetByExpressionAsync(o => o.OtpCode == otpCode && o.Purpose == otpPurpose && !o.IsUsed);
            if (otp == null)
                return ApiResponse<Otp>.Failure(StatusCodes.Status404NotFound, "otp not found") ;

            if (otp.IsUsed)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "otp already used");

            if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "already expires");
            //mark otp has used
            otp.IsUsed = true;
             _uow.OtpRepo.Update(otp);
            await _uow.CompleteAsync();
            return ApiResponse<Otp>.Success("success", otp); 
        }

        public async Task<string> GenerateOtpAsync(string userId, OtpPurpose purpose, bool includeAlphabet, string channel)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            string otpCode = OtpUtility.GenerateRandomCode(6, includeAlphabet);

            var otp = new Otp
            {
                UserId = user.Id,
                OtpCode = otpCode,
                Purpose = purpose.ToString(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                IsUsed = false,
                Channel = channel   
            };

            await _uow.OtpRepo.AddAsync(otp);
            await _uow.CompleteAsync();
            return otpCode;
        }
    }
}
