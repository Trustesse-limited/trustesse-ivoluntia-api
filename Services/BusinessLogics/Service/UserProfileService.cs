using Azure.Core;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.DTOs.UserProfile;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class UserProfileService: IUserProfileService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private ICurrentUserService _currentUserService;
        public UserProfileService(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }
        public async Task<GlobalRequestReponse<VolunteerProfileDto>> VolunteerProfile()
        {
            var user = await _unitOfWork.userRepo.GetByExpressionIncludeAsync(u => u.Email == _currentUserService.GetUserEmail(), u => u.Interests, u => u.Skills, u => u.Location, u => u.Location.Country, u => u.Location.State, u => u.Qualifications);
            var userQualification = await _unitOfWork.userQualificationRepo.GetListByExpressionAsync(u => u.UserId == _currentUserService.GetUserId(), u => u.Qualification);
            var id = _currentUserService.GetUserId();
            if (user == null)
            {
                return ResponseHelper.BuildResponse<VolunteerProfileDto>("user not found , please sign up", StatusCodes.Status404NotFound, null, false);
            }
            var profileResponse = _mapper.Map<VolunteerProfileDto>(user);
            profileResponse.Address = user.Location?.Address ?? null;
            profileResponse.Gender = user.Gender ?? null;
            profileResponse.Email = user.Email ?? null;
            profileResponse.Bio = user.Bio ?? null;
            profileResponse.ImageUrl = user.UserImage ?? null;
            profileResponse.City = user.Location?.City ?? null;
            profileResponse.DateOfBirth = user?.DateOfBirth ?? null; 
            profileResponse.ZipCode = user.Location?.Zipcode ?? null;
            profileResponse.Country = user.Location?.Country.CountryName ?? null;
            profileResponse.State = user.Location?.State.StateName ?? null;
            profileResponse.Interests = user.Interests?.Select(i => i.Name).ToList() ?? null;
            profileResponse.Skills = user.Skills?.Select(s => s.Name).ToList() ?? null;
            profileResponse.QualificationTitle = user.Qualifications?.Select(q => q.Title).ToList()?? null;
            profileResponse.QualificationUrl= user.UserQualifications?.Select(uq => uq.ProofOfQualificationURL).ToList() ?? null;
            return ResponseHelper.BuildResponse<VolunteerProfileDto>("successful", StatusCodes.Status200OK, profileResponse, true);
        }
        public async Task<GlobalRequestReponse<FoundationProfileDto>> FoundationProfile()
        {
            var user = await _unitOfWork.userRepo.GetByExpressionIncludeAsync(u => u.Email == _currentUserService.GetUserEmail(), u => u.Foundation, u => u.Foundation.Category, u => u.Foundation.Location.State, u => u.Foundation.Location.Country, u => u.Foundation.Causes);
            if (user == null)
            {
                return ResponseHelper.BuildResponse<FoundationProfileDto>("foundation not found , please sign up", StatusCodes.Status404NotFound, null, false);
            }
            var profileResponse = _mapper.Map<FoundationProfileDto>(user);
            profileResponse.Address = user.Location?.Address ?? null;
            profileResponse.Email = user.Foundation?.Email ?? null; 
            profileResponse.City = user.Foundation?.Location?.City ?? null;
            profileResponse.Mission = user.Foundation?.Mission ?? null; 
            profileResponse.Name = user.Foundation?.Name ?? null;  
            profileResponse.LogoUrl = user.Foundation?.Logo ?? null;
            profileResponse.Website = user.Foundation?.Website ?? null;
            profileResponse.FoundationCategory = user.Foundation?.Category?.Name ?? null;
            profileResponse.Zipcode = user.Foundation?.Location?.Zipcode ?? null;
            profileResponse.FoundationCountry = user.Foundation?.Location?.Country?.CountryName ?? null;
            profileResponse.FoundationState = user.Foundation?.Location?.State?.StateName ?? null;
            profileResponse.Causes = user.Foundation?.Causes?.Select(i => i.Name).ToList() ?? null;
            profileResponse.HasAgreedToDisclaimer = user.Foundation.HasAgreedToDisclaimer;
            return ResponseHelper.BuildResponse<FoundationProfileDto>("successful", StatusCodes.Status200OK, profileResponse, true);
        }

        public async Task<GlobalRequestReponse<string>> UpdateVolunteerProfile(VolunteerProfileUpdateDto volunteerProfileUpdateDto)
        {
            var user = await _unitOfWork.userRepo.GetByExpressionIncludeAsync(u => u.Email == _currentUserService.GetUserEmail());
            if (user == null)
            {
                return ResponseHelper.BuildResponse<string>("user not found , please sign up", StatusCodes.Status404NotFound, null, false);
            }
            user.FirstName = volunteerProfileUpdateDto.FirstName;
            user.LastName = volunteerProfileUpdateDto.LastName; 
            user.Email = volunteerProfileUpdateDto.Email;
            _unitOfWork.userRepo.Update(user);
            var save = await _unitOfWork.CompleteAsync(); 
            if(save > 0)
                return ResponseHelper.BuildResponse<string>("updated successfully", StatusCodes.Status200OK,null, true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        public async Task<GlobalRequestReponse<string>> UpdateFoundationProfile(FoundationProfileUpdateDto foundationProfileUpdateDto)
        {
            var user = await _unitOfWork.userRepo.GetByExpressionIncludeAsync(u => u.Email == _currentUserService.GetUserEmail(), u => u.Foundation);
            if (user == null)
            {
                return ResponseHelper.BuildResponse<string>("foundation not found , please sign up", StatusCodes.Status404NotFound, null, false);
            }
            user.Email = foundationProfileUpdateDto.Email;
            user.Foundation.Name = foundationProfileUpdateDto.Name;
            user.Foundation.Website = foundationProfileUpdateDto.Website;
            _unitOfWork.userRepo.Update(user);
            var save = await _unitOfWork.CompleteAsync();
            if (save > 0)
                return ResponseHelper.BuildResponse<string>("updated successfully", StatusCodes.Status200OK, null, true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
