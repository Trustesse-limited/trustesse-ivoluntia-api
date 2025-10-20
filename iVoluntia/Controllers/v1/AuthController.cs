using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IEmailService _email;
        public AuthController(IAuthService authService, IEmailService email)
        {
            _authService = authService;
            _email = email;

        }

        [HttpPost("volunteer")]
        public async Task<IActionResult> CreateVolunteer([FromBody] VolunteerSignUpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _authService.CreateVolunteer(request);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }

        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var result = await _authService.ResetPasswordAsync(email);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _authService.ChangePasswordAsync(request);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
        [HttpPost("validateotp")]
        public async Task<IActionResult> ValidateOTP(string otp, string email)
        {
            var result = await _authService.ValidateOTP(otp, email);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
        [HttpPost("resendotp")]
        public async Task<IActionResult> ResendOTP(string email)
        {
            var result = await _authService.ResendOTP(email);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
        [HttpPost("createpassword")]
        public async Task<IActionResult> CreatePassword(ResetPasswordModel model)
        {
            var result = await _authService.CreatePasswordAsync(model);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
    }
}
