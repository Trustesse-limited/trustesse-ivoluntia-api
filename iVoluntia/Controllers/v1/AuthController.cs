using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(IAuthenticationService authenticationService, IAuthService authService) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
        {
            var response = await authenticationService.LoginAsync(request, cancellationToken);
            return response.ToActionResult();
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken)
        {
            var response = await authenticationService.RefreshTokenAsync(request, cancellationToken);
            return response.ToActionResult();
        }

        [HttpPost("volunteer")]
        public async Task<IActionResult> CreateVolunteer([FromBody] VolunteerSignUpDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await authService.CreateVolunteer(request);

            if (result.StatusCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }

            return Ok(result);
        }
    }
}
