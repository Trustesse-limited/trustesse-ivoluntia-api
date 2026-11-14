using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Trustesse.Ivoluntia.Commons.Contants;
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
        [HttpGet("get-volunteer-profile")]
        public async Task<IActionResult> VolunteerProfile()
            => BuildHttpResponse<VolunteerProfileDto>(await _userProfileService.VolunteerProfile());

        [HttpGet("get-foundation-profile")]
        public async Task<IActionResult> FoundationProfile()
            => BuildHttpResponse<FoundationProfileDto>(await _userProfileService.FoundationProfile());

        [HttpPut("update-volunteer-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateVolunteerProfile(VolunteerProfileUpdateDto volunteerProfileUpdateDto)
            => BuildHttpResponse<string>(await _userProfileService.UpdateVolunteerProfile(volunteerProfileUpdateDto.Validate()));

        [HttpPut("update-foundation-profile")]
        [Authorize(Roles = AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> UpdateFoundationProfile(FoundationProfileUpdateDto foundationProfileUpdateDto)
            => BuildHttpResponse<string>(await _userProfileService.UpdateFoundationProfile(foundationProfileUpdateDto.Validate()));
    }
}
