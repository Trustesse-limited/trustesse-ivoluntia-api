using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VolunteersController : BaseController
    {
        private readonly IVolunteerService _volunteerService;
        public VolunteersController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }

        [HttpGet("get-volunteer-by-foundation-id")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> GetVolunteers([FromQuery] VolunteerQueryDto query)
            => BuildHttpResponse(await _volunteerService.GetVolunteers(query.FoundationId, query.IsActive));
    }
}
