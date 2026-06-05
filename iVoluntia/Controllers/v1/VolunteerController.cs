using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
    public class VolunteersController : ControllerBase
    {
        private readonly IVolunteerService _volunteerService;
        public VolunteersController(IVolunteerService volunteerService)
        {
            _volunteerService = volunteerService;
        }


        [HttpGet("get-volunteer-by-foundation-id")]
        [Authorize(Roles = "SuperAdmin, FoundationAdmin")]
        public async Task<IActionResult> GetVolunteers([FromQuery] VolunteerQueryDto query)
        {
            var result = await _volunteerService.GetVolunteers(query.FoundationId, query.IsActive);

            return Ok(result);
        }
    }
}
