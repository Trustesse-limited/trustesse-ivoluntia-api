using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class OnboardingService : IOnboardingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IFileUploadService _fileUploadService;
        private readonly ICurrentUserService _currentUserService;
        public OnboardingService(IUnitOfWork uow, IMapper mapper, UserManager<User> userManager, IUserRepository userRepository, IFileUploadService fileUploadService, ICurrentUserService currentUserService)
        {
            _uow = uow;
            _mapper = mapper;
            _userManager = userManager;
            _userRepository = userRepository;
            _fileUploadService = fileUploadService;
            _currentUserService = currentUserService;
        }
        public async Task<GlobalRequestReponse<OnboardingResponseDto>> CreateVolunterOnboarding(VolunteerOnboardingRequestDto volunteerOnboardingDto)
        {
            int pageRemaining = 5 - volunteerOnboardingDto.onboardingMetaData.CurrentPage;
            bool hasCompleteOnboarding = false;
            switch (volunteerOnboardingDto.onboardingMetaData.CurrentPage)
            {
                case 1:
                    await UpdateBioData(volunteerOnboardingDto.BioData);
                    break;
                case 2:
                    await UpdateLocation(volunteerOnboardingDto.LocationDto);
                    break;
                case 3:
                    await UpdateUserInterest(volunteerOnboardingDto.Interest);
                    break;
                case 4:
                    await UpdateUserSkill(volunteerOnboardingDto.Skill);
                    break;
                case 5:
                    var imageUrl = await _fileUploadService.UploadFilesAsync(volunteerOnboardingDto.ProfileAndBioData.ProfileImage);
                    var user = await _uow.userRepo.GetByExpressionAsync(u => u.Email == _currentUserService.GetUserEmail());
                    user.UserImage = imageUrl.Data[0];
                    await _userManager.UpdateAsync(user);
                    await AddOnBoardingProgress(user.Id, (int)OnBoardingPages.ProfileImageAndBio, true, 5);
                    hasCompleteOnboarding = true;
                    break;
                default:
                    return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), false);
            }
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
        }
        public async Task<GlobalRequestReponse<OnboardingResponseDto>>CreateOrganizationOnboarding(OrganizationOnboardingRequestDto organizationOnboardingDto)
        {
            int pageRemaining = 5 - organizationOnboardingDto.MetaData.CurrentPage;
            bool hasCompleteOnboarding = false;
            switch (organizationOnboardingDto.MetaData.CurrentPage)
            {
                case 1:
                    var mapOrganization = _mapper.Map<Foundation>(organizationOnboardingDto.foundationBioData);
                    mapOrganization.Email = _currentUserService.GetUserEmail();
                    var category = await _uow.CategoryRepository.GetByExpressionAsync(c => c.Name == organizationOnboardingDto.foundationBioData.FoundationCategory);
                    mapOrganization.CategoryId = category.Id;
                    var foundationAdmin = await _userManager.FindByEmailAsync(_currentUserService.GetUserEmail());
                    foundationAdmin.FoundationId = mapOrganization.Id;
                    mapOrganization.Status = OrganizationStatusUpdateEnums.Pending.ToString();
                    await _uow.OrganizationRepository.AddAsync(mapOrganization);
                    await _uow.CompleteAsync();
                    var response = await _userManager.UpdateAsync(foundationAdmin);
                    await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.BioDataPage, false);
                    break;
                case 2:
                    var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    organizationOnboardingDto.FoundationLocationDto.UserId = _currentUserService.GetUserId();
                    var locationId = await AddLocation(organizationOnboardingDto.FoundationLocationDto, foundation.Id);
                    foundation.LocationId = locationId;
                    _uow.OrganizationRepository.Update(foundation);
                    await _uow.CompleteAsync();
                    await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Location, false);
                    break;
                case 3:
                    var foundationMap = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    await AddFoundationCause(organizationOnboardingDto.CauseDto.Names, foundationMap.Id, _currentUserService.GetUserEmail());
                    await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Cause, false);
                    break;
                case 4:
                    var imageUrl = await _fileUploadService.UploadFilesAsync(organizationOnboardingDto.ProfileLogo.Logo);
                    var foundationMapp = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    foundationMapp.Logo = imageUrl.Data[0];
                    _uow.OrganizationRepository.Update(foundationMapp);
                    await _uow.CompleteAsync();
                    await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Profile, false);
                    break;
                case 5:
                    var foundationMapping = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    foundationMapping.HasAgreedToDisclaimer = organizationOnboardingDto.Disclaimer.HasAgreedToDisclaimer;
                    foundationMapping.HasAgreedToDisclaimer = organizationOnboardingDto.Disclaimer.HasAgreedToDisclaimer;
                    foundationMapping.IsActive = true;
                    _uow.OrganizationRepository.Update(foundationMapping);
                    await _uow.CompleteAsync();
                    await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Disclaimer, true);
                    hasCompleteOnboarding = true;
                   break;
                default:
                    return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), false);
            }
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
        }

        public async Task<ApiResponse<string>> UpdateBioData(BioData model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            volunteer.FirstName = model.FirstName;
            volunteer.LastName = model.LastName;
            volunteer.Gender = model.Gender;
            volunteer.DateOfBirth = model.DateOfBirth;
            var bioUpdate = await _userManager.UpdateAsync(volunteer);
            if (bioUpdate.Succeeded)
            {
                await UpdateOnBoardingProgress(volunteer.Id, 1, false);
            }
            return ApiResponse<string>.Success("Volunteer BioData updated successfully.", null);
        }

        public async Task<ApiResponse<string>> UpdateLocation(LocationDto model)
        {
            var country = await _uow.countryRepo.GetByExpressionAsync(c => c.CountryName == model.Country);
            if (country == null)
                return ApiResponse<string>.Failure(404, $"Country doesn't exist.");
            var state = await _uow.stateRepo.GetByExpressionAsync(s => s.StateName == model.State);
            if (state == null)
                return ApiResponse<string>.Failure(404, $"State doesn't exist.");
            var location = new Location
            {
                CountryId = country.Id,
                StateId = state.Id,
                City = model.City,
                Zipcode = model.ZipCode,
                Address = model.Address,
                UserId = _currentUserService.GetUserId()
            };
            await _uow.locationRepo.AddAsync(location);
            var rowchange = await _uow.CompleteAsync();
            if (rowchange > 0)
            {
                await UpdateOnBoardingProgress(_currentUserService.GetUserId(), 2, false);
            }
            return ApiResponse<string>.Success("Volunteer Location updated successfully.", null);
        }

        public async Task<ApiResponse<string>> UpdateUserInterest(InterestDto model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            if (model.Names.Any())
            {
                foreach (var name in model.Names)
                {
                    var interestExist = await _uow.interestRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                    var saveUserInterest = new UserInterestLink()
                    {
                        UserId = volunteer.Id,
                        InterestId = interestExist.Id
                    };
                    await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                    await _uow.CompleteAsync();
                }
            }
            var response = await UpdateOnBoardingProgress(volunteer.Id, 3, false);
            if (response.StatusCode == StatusCodes.Status200OK)
                return ApiResponse<string>.Success("onboarding updated", response.Data);
            return ApiResponse<string>.Success("unable to update onboarding.", null);
        }

        public async Task<ApiResponse<string>> UpdateUserSkill(VolunteerSkillDto model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            if (model.Names.Any())
            {
                foreach (var name in model.Names)
                {
                    var skill = await _uow.skillRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                    {
                        var skillMap = new UserSkillLink()
                        {
                            UserId = volunteer.Id,
                            SkillId = skill.Id
                        };
                        await _uow.userSkillLinkRepo.AddAsync(skillMap);
                        await _uow.CompleteAsync();
                    }
                }
            }
            var response = await UpdateOnBoardingProgress(volunteer.Id, 4, false);
            if (response.StatusCode == StatusCodes.Status200OK)
                return ApiResponse<string>.Success("onboarding update", response.Data);
            return ApiResponse<string>.Success("unable to update onboarding.", null);
        }

        public async Task<ApiResponse<string>> UpdateProfileImageAndBio(ProfileImageAndBio model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            volunteer.Bio = model.Bio;
            var profile = await _userManager.UpdateAsync(volunteer);
            if (profile.Succeeded)
            {
                await UpdateOnBoardingProgress(volunteer.Id, 6, true);
            }
            return ApiResponse<string>.Success("Volunteer Bio and Image updated successfully.", null);
        }

        public async Task<ApiResponse<string>> UpdateOnBoardingProgress(string userId, int lastCompletedPage, bool hasCompletedOnboarding)
        {
            var updateProgressTable = await _uow.onboardingProgressRepo.GetByExpressionAsync(x => x.UserId == userId);
            if (updateProgressTable != null)
            {
                updateProgressTable.UserId = userId.ToString();
                updateProgressTable.LastCompletedPage = lastCompletedPage;
                updateProgressTable.HasCompletedOnboarding = hasCompletedOnboarding;

                await _uow.onboardingProgressRepo.UpdateAsync(updateProgressTable);
                if (await _uow.CompleteAsync() > 0)
                {
                    return ApiResponse<string>.Success("OnboardingProgress has been updated successfully.", null);
                }
            }
            return ApiResponse<string>.Success("OnboardingProgress has been updated successfully.", null);
        }

        public async Task<ApiResponse<string>> AddOnBoardingProgress(string userId, int lastCompletedPage, bool hasCompleteOnboarding, int totalPages)
        {
            var onboardingPorgress = new OnboardingProgress
            {
                UserId = userId,
                TotalPages = totalPages,
                LastCompletedPage = lastCompletedPage,
                HasCompletedOnboarding = hasCompleteOnboarding
            };
            await _uow.onboardingProgressRepo.AddAsync(onboardingPorgress);
            var succeed = await _uow.CompleteAsync();
            if (succeed > 0)
                return ApiResponse<string>.Success("onboarding progress added", "success");
            return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "unable to add onboarding");
        }
        public async Task<ApiResponse<string>> AddFoundationCause(List<string> causeName, string foundationId, string foundationAdminEmail)
        {
            var causes = await _uow.CauseRepository
                .GetAsync(c => causeName.Contains(c.Name));
            if (causes == null || !causes.Any())
            {
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "No matching causes found");
            }
            var foundationCauses = causes.Select(cause => new FoundationCauses
            {
                CauseId = cause.Id,
                FoundationId = foundationId,
                CreatedBy = foundationAdminEmail
            }).ToList();

            await _uow.CauseFoundationRepository.AddManyAsync(foundationCauses);
            await _uow.CompleteAsync();
            return ApiResponse<string>.Success("foundation causes added successfully", "foundation cause added");
        }

        public async Task<string> AddLocation(FoundationLocationDto foundationLocationDto, string foundationId)
        {
            var country = await _uow.countryRepo.GetByExpressionAsync(c => c.CountryName == foundationLocationDto.FoundationCountry);
            if (country == null)
                return ($"Country doesn't exist.");
            var state = await _uow.stateRepo.GetByExpressionAsync(s => s.StateName == foundationLocationDto.FoundationState);
            if (state == null)
                return ("State doesn't exist.");
            foundationLocationDto.StateId = state.Id;
            foundationLocationDto.CountryId = country.Id;
            var mapLocation = _mapper.Map<Location>(foundationLocationDto);
            mapLocation.FoundationId = foundationId;
            await _uow.locationRepo.AddAsync(mapLocation);
            var rowchange = await _uow.CompleteAsync();
            return mapLocation.Id;
        }
    }
}
