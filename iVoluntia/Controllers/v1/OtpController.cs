using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class OtpController : BaseController
    {
        private readonly IOtpService _otpService;
        private readonly IAuthService _authService;
        private readonly ICurrentUserService _currentUserService;
        public OtpController(IOtpService otpService, IAuthService authService, ICurrentUserService currentUserService)
        {
            _otpService = otpService;
            _authService = authService;
            _currentUserService = currentUserService;
        }
        [HttpPost("generate-otp")]
        public async Task<IActionResult> GenerateOtp([FromBody] GenerateOtpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));
            if (!Enum.TryParse<OtpPurpose>(request.Purpose.ToString(), true, out var purposeEnum))
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid purpose."));
            var code = await _otpService.GenerateOtpAsync(_currentUserService.GetUserId(), purposeEnum.ToString(), request.IncludeAlphabet, request.Channel);
            return Ok(new { Message = "OTP generated successfully and sent.", OtpCode = code });
        }

        [HttpPost("confirm-otp")]
        public async Task<IActionResult> ConfirmOtp([FromBody] ConfirmOtpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var isValid = await _otpService.ConfirmOtpAsync(request.OtpCode, request.Purpose.ToString());

            if (isValid.StatusCode == StatusCodes.Status400BadRequest)
            {
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid or expired OTP."));
            }

            return Ok(ApiResponse<string>.Success("OTP confirmed.", null));
        }

        [HttpPost("resendotp")]
        public async Task<IActionResult> ResendOTP(string email, string purpose, bool includeAlphabet, string notificationType)
            => BuildHttpResponse<string>(await _otpService.ResendOTP(email, purpose, includeAlphabet, NotificationChannelEnum.Email.ToString(), notificationType));
       
        [EnableRateLimiting("fixed")]
        [HttpPost("verify-email-confirm-otp")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string otpCode)
            => BuildHttpResponse<string>(await _otpService.ConfirmEmail(otpCode, OtpPurpose.Signup.ToString()));

        [EnableRateLimiting("fixed")]
        [HttpPost("verify-reset-password-otp")]
        public async Task<IActionResult> VerifyResetPasswordOtp([FromQuery] string otpCode)
            => BuildHttpResponse<string>(await _otpService.VerifyResetPasswordOtp(otpCode));

        [EnableRateLimiting("fixed")]
        [HttpPost("verify-forgot-password-otp")]
        public async Task<IActionResult> VerifyForgotPasswordOtp([FromQuery] string otpCode)
            => BuildHttpResponse<string>(await _otpService.VerifyForgotPasswordOtp(otpCode));
    }
}
