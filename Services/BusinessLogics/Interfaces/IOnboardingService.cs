using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IOnboardingService
    {
        Task<GlobalRequestReponse<OnboardingResponseDto>> CreateVolunterOnboarding(VolunteerOnboardingRequestDto volunteerOnboardingDto);
        Task<GlobalRequestReponse<OnboardingResponseDto>> CreateOrganizationOnboarding(OrganizationOnboardingRequestDto organizationOnboardingDto);
    }
}
