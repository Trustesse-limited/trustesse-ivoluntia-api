using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionsController : ControllerBase
    {
        private readonly ISecurityQuestionService _securityQuestionService;

        public SecurityQuestionsController(ISecurityQuestionService securityQuestionService)
        {
            _securityQuestionService = securityQuestionService;
        }

        [HttpPost("security-questions")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> AddSecurityQuestion([FromBody] CreateSecurityQuestionRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _securityQuestionService.AddSecurityQuestion(request);

            return Ok(result);
        }

        [HttpGet("security-questions")]
        public async Task<IActionResult> GetSecurityQuestions()
        {
            var result = await _securityQuestionService.GetSecurityQuestions();

            return Ok(result);
        }

        [HttpDelete("security-questions/{id}")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> RemoveSecurityQuestion(string id)
        {
            if (id == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _securityQuestionService.RemoveSecurityQuestion(id);

            return Ok(result);
        }

        [HttpPost("users/security-questions/setup")]
        public async Task<IActionResult> SetupSecurityQuestions([FromBody] SetupSecurityQuestionsRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _securityQuestionService.SetupSecurityQuestionsAsync(request);

            return Ok(result);
        }

        [HttpPost("users/security-questions/validate")]
        public async Task<IActionResult> ValidateSecurityQuestions([FromBody] ValidateSecurityQuestionsRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _securityQuestionService.ValidateSecurityQuestionsAsync(request);

            return Ok(result);
        }

        [HttpPost("users/security-questions/reset-request")]
        public async Task<IActionResult> RequestResetSecurityQuestions()
        {
            var result = await _securityQuestionService.RequestSecurityQuestionResetAsync();

            return Ok(result);
        }

        [HttpPost("users/security-questions/reset")]
        public async Task<IActionResult> ResetSecurityQuestions([FromBody] ResetSecurityQuestionsRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _securityQuestionService.ResetSecurityQuestionsAsync(request);

            return Ok(result);
        }
    }
}
