using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.DTOs.UserProfile;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : BaseController
    {
        private readonly IUserProfileService _userProfileService;
        public UserProfileController(IUserProfileService userProfileService)
        {
            _userProfileService = userProfileService;
        }
        [HttpGet("volunteer-profile")]
        public async Task<IActionResult> VolunteerProfile()
            => BuildHttpResponse<VolunteerProfileDto>(await _userProfileService.VolunteerProfile());

        [HttpGet("foundation-profile")]
        public async Task<IActionResult> FoundationProfile()
            => BuildHttpResponse<FoundationProfileDto>(await _userProfileService.FoundationProfile());
    }
}
