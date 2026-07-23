using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OnboardingController : BaseController
    {
        private readonly IOnboardingService _onboardingService;
        public OnboardingController(IOnboardingService onboardingService)
        {
            _onboardingService = onboardingService;
        }
        [HttpPost("volunteer-onboarding")]
        public async Task<IActionResult> VolunteerOnboarding([FromForm] VolunteerOnboardingRequestDto volunteerOnboardingDto)
             => BuildHttpResponse<OnboardingResponseDto>(await _onboardingService.CreateVolunterOnboarding(volunteerOnboardingDto.Validate()));

        [HttpPost("organization-onboarding")]
        public async Task<IActionResult> OrganizationOnboarding([FromForm] OrganizationOnboardingRequestDto organizationOnboardingDto)
            => BuildHttpResponse<OnboardingResponseDto>(await _onboardingService.CreateOrganizationOnboarding(organizationOnboardingDto.Validate()));
    }
}
