using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Location;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LocationController : BaseController
    {
        private readonly ILocationService _locationService;
        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost("create-location")]
        public async Task<IActionResult> CreateLocation([FromBody] CreateLocationRequestDto request)
            => BuildHttpResponse(await _locationService.CreateLocation(request.Validate()));
    }
}
