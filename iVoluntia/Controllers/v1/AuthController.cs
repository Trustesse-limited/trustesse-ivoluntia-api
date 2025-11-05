using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
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


        [HttpPost("volunteer")]
        public async Task<IActionResult> CreateVolunteer([FromBody] VolunteerSignUpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _authenticationService.CreateVolunteer(request);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }

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
        public async Task<IActionResult> ConfirmUser(ConfirmUserModel model)
        {
            var result = await _authenticationService.ConfirmUser(model);

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
