using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.DTOs.UserProfile;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IUserProfileService
    {
        Task<GlobalRequestReponse<VolunteerProfileDto>> VolunteerProfile();
        Task<GlobalRequestReponse<FoundationProfileDto>> FoundationProfile();
    }
}
