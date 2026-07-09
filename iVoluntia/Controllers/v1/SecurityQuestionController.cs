using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityQuestionsController : BaseController
    {
        private readonly ISecurityQuestionService _securityQuestionService;

        public SecurityQuestionsController(ISecurityQuestionService securityQuestionService)
        {
            _securityQuestionService = securityQuestionService;
        }

        [HttpPost("security-questions")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> AddSecurityQuestion([FromBody] CreateSecurityQuestionRequest request)
            => BuildHttpResponse(await _securityQuestionService.AddSecurityQuestion(request.Validate()));

        [HttpGet("security-questions")]
        public async Task<IActionResult> GetSecurityQuestions()
            => BuildHttpResponse(await _securityQuestionService.GetSecurityQuestions());

        [HttpDelete("security-questions/{id}")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> RemoveSecurityQuestion(string id)
            => BuildHttpResponse(await _securityQuestionService.RemoveSecurityQuestion(id));

        [HttpPost("users/security-questions/setup")]
        public async Task<IActionResult> SetupSecurityQuestions([FromBody] SetupSecurityQuestionsRequest request)
            => BuildHttpResponse(await _securityQuestionService.SetupSecurityQuestionsAsync(request));

        [HttpPost("users/security-questions/validate")]
        public async Task<IActionResult> ValidateSecurityQuestions([FromBody] ValidateSecurityQuestionsRequest request)
            => BuildHttpResponse(await _securityQuestionService.ValidateSecurityQuestionsAsync(request));

        [HttpPost("users/security-questions/reset-request")]
        public async Task<IActionResult> RequestResetSecurityQuestions()
            => BuildHttpResponse(await _securityQuestionService.RequestSecurityQuestionResetAsync());

        [HttpPost("users/security-questions/reset")]
        public async Task<IActionResult> ResetSecurityQuestions([FromBody] ResetSecurityQuestionsRequest request)
            => BuildHttpResponse(await _securityQuestionService.ResetSecurityQuestionsAsync(request.Validate()));
    }
}
