using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Qualification;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class QualificationController : BaseController
    {
        private readonly IQualificationService _qualificationService;

        public QualificationController(IQualificationService qualificationService)
        {
            _qualificationService = qualificationService;
        }

        [HttpPost("creation")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> CreateQualification([FromBody] CreateQualificationDto request)
            => BuildHttpResponse(await _qualificationService.CreateQualification(request));

        [HttpPut("{qualificationId}")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> UpdateQualification(string qualificationId, [FromBody] UpdateQualificationDto request)
            => BuildHttpResponse(await _qualificationService.UpdateQualification(qualificationId, request));

        [HttpGet]
        public async Task<IActionResult> GetAllQualifications()
            => BuildHttpResponse(await _qualificationService.GetAllQualifications());

        [HttpGet("{qualificationId}")]
        public async Task<IActionResult> GetQualificationById(string qualificationId)
            => BuildHttpResponse(await _qualificationService.GetQualificationById(qualificationId));

        [HttpDelete("{qualificationId}")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin)]
        public async Task<IActionResult> DeleteQualification(string qualificationId)
            => BuildHttpResponse(await _qualificationService.DeleteQualification(qualificationId));
    }
}
