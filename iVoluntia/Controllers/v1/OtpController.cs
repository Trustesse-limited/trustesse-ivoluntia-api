using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/otps")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        private readonly IAuthService _authService;
        public OtpController(IOtpService otpService, IAuthService authService)
        {
            _otpService = otpService;
            _authService = authService;
        }
        [HttpPost("generate-otp")]
        public async Task<IActionResult> GenerateOtp([FromBody] GenerateOtpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            if (!Enum.TryParse<OtpPurpose>(request.Purpose.ToString(), true, out var purposeEnum))
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid purpose."));

            var code = await _otpService.GenerateOtpAsync(request.UserId, purposeEnum);


            return Ok(new { Message = "OTP generated successfully and sent.", OtpCode = code });
        }

        [HttpPost("confirm-otp")]
        public async Task<IActionResult> ConfirmOtp([FromBody] ConfirmOtpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var isValid = await _otpService.ConfirmOtpAsync(request.UserId, request.OtpCode, request.Purpose);

            if (!isValid)
            {
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid or expired OTP."));
            }

            return Ok(ApiResponse<string>.Success("OTP confirmed.", null));
        }

        [HttpPost("resendotp")]
        public async Task<IActionResult> ResendOTP(string email, OtpPurpose purpose)
        {
            var result = await _authService.ResendOTP(email, purpose);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
    }
}
