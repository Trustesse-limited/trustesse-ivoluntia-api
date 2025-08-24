using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.API.Extensions;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Services.Abstractions;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController(IAuthenticationService authenticationService) : ControllerBase
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
    }
}
