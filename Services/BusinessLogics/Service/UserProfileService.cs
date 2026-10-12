using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.DTOs.UserProfile;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class UserProfileService: IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserProfileService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<GlobalRequestReponse<VolunteerProfileDto>> VolunteerProfile()
        {
            return null;
        }
        public async Task<GlobalRequestReponse<FoundationProfileDto>> FoundationProfile()
        {
            return null;
        }

    }
}
