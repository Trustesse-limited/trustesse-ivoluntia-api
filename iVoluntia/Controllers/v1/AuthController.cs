using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/countries")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
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
    }
}
