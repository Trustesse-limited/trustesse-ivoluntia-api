using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
        {
            var response = await _authenticationService.LoginAsync(request, cancellationToken);
            return response.ToActionResult();
        }

        [HttpPost("volunteer-signup")]
        public async Task<IActionResult> CreateVolunteer([FromForm] VolunteerSignUpDto request)
            =>BuildHttpResponse<string>(await _authenticationService.CreateVolunteer(request.Validate()));    
        
        [HttpPost("organization-signup")]
        public async Task<IActionResult> CreateOrganization([FromForm] CreateFoundationRequestDto createFoundationRequestDto)
            => BuildHttpResponse<string>(await _authenticationService.CreateOrganization(createFoundationRequestDto.Validate()));
       
        [HttpPost("resetpassword")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            var result = await _authenticationService.ResetPasswordAsync(email);

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

            var result = await _authenticationService.ChangePasswordAsync(request);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
        [HttpPost("confirmuser")]
        public async Task<IActionResult> ConfirmUser([FromQuery]string otpCode)
        {
            var result = await _authenticationService.ConfirmUser(otpCode, OtpPurpose.Signup.ToString());

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }

        [HttpPost("createpassword")]
        public async Task<IActionResult> CreatePassword(ResetPasswordModel model)
        {
            var result = await _authenticationService.CreatePasswordAsync(model);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }

    }
}
