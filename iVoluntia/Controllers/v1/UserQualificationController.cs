using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.UserQualification;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/user-qualifications")]
    [ApiController]
    [Authorize]
    public class UserQualificationController : BaseController
    {
        private readonly IUserQualificationService _userQualificationService;

        public UserQualificationController(IUserQualificationService userQualificationService)
        {
            _userQualificationService = userQualificationService;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserQualification([FromForm] CreateUserQualificationRequestDto request)
            => BuildHttpResponse(await _userQualificationService.AddUserQualification(request));
    }
}
