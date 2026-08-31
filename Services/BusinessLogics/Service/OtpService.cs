using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using Trustesse.Ivoluntia.Commons.Cryptography;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
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
        private readonly IOtpEmailSenderService _otpEmailSenderService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly byte[] _key;
        private readonly string _text;
        public OtpService(UserManager<User> userManager, IUnitOfWork uow, ICurrentUserService currentUserService, IConfiguration configuration, IMapper mapper, IOtpEmailSenderService otpEmailSenderService)
        {
            _userManager = userManager;
            _uow = uow;
            _currentUserService = currentUserService;
            _configuration = configuration;
            _text = configuration["PasswordResetToken:Text"];
            var key = configuration["PasswordResetToken:Key"];
            _key = Convert.FromBase64String(key);
            _mapper = mapper;
            _otpEmailSenderService = otpEmailSenderService;
        }
        public async Task<ApiResponse<Otp>> ConfirmOtpAsync(string otpCode, string otpPurpose)
        {
            var otp = await _uow.OtpRepo.GetByExpressionAsync(o => o.OtpCode == otpCode && o.Purpose == otpPurpose && !o.IsUsed);
            if (otp == null)
                return ApiResponse<Otp>.Failure(StatusCodes.Status404NotFound, "otp not found") ;

            if (otp.IsUsed)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "otp already used");

            if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 10)
                return ApiResponse<Otp>.Failure(StatusCodes.Status400BadRequest, "already expires");
            //mark otp has used
            otp.IsUsed = true;
             _uow.OtpRepo.Update(otp);
            await _uow.CompleteAsync();
            return ApiResponse<Otp>.Success("success", otp); 
        }
        public async Task<GlobalRequestReponse<string>> ConfirmEmail(string otpCode, string otpPurpose)
        {
            var otp = await ConfirmOtpAsync(otpCode, otpPurpose);
            if (otp.StatusCode != StatusCodes.Status200OK)
                return ResponseHelper.BuildResponse<string>(otp.Message, otp.StatusCode, null, false);
            var user = await _userManager.FindByIdAsync(otp.Data.UserId);
            if (user != null)
            {
                user.EmailConfirmed = true;
                user.IsActive = true;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return ResponseHelper.BuildResponse<string>(otp.Message, otp.StatusCode, "Account created Successfully", true);
                }
                return ResponseHelper.BuildResponse<string>(otp.Message, otp.StatusCode, "something went wrong", false);
            }
            return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, "Wrong OTP. no user found", false);
        }
        public async Task<GlobalRequestReponse<string>> VerifyResetPasswordOtp(string otpCode)
        {
            var otp = await ConfirmOtpAsync(otpCode, OtpPurpose.PasswordReset.ToString());
            if (otp.StatusCode != StatusCodes.Status200OK)
                return ResponseHelper.BuildResponse<string>(otp.Message, otp.StatusCode, null, false);
            var user = await _userManager.FindByIdAsync(otp.Data.UserId);
            if (user != null)
            {
                var tokenPayload = _mapper.Map<PasswordResetTokenPayload>(user);
                tokenPayload.Text = _text;
                var json = JsonSerializer.Serialize(tokenPayload);
                var token = AES.EncryptData(json, _key);
                return ResponseHelper.BuildResponse<string>("otp verified and token generated", StatusCodes.Status200OK, $"token:{token}", true);
            }
            return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, "Wrong OTP. no user found", false);
        }
        public async Task<GlobalRequestReponse<string>> ResendOTP(string email, string purpose, bool includeAlphabet, string channel, string notificationType)
        {
            var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
            if (user != null)
            {
                //Generate OTP
                var otp = await GenerateOtpAsync(user.Id, purpose, includeAlphabet, channel);
                user.OTP = otp;
                user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                var result = await _userManager.UpdateAsync(user);
                if (!result.Succeeded)
                {
                    return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status404NotFound, null, false);
                }
                else
                {
                    var response = await _otpEmailSenderService.OtpSender(user.Email, user.OTP, notificationType);
                    return response;
                }   
            }
            return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
        }
        public async Task<string> GenerateOtpAsync(string userId, string purpose, bool includeAlphabet, string channel)
        {
            string otpCode = OtpUtility.GenerateRandomCode(6, includeAlphabet);
            var otp = new Otp
            {
                UserId = userId,
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
