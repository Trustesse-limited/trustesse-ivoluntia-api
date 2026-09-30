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
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using BioData = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.BioData;
using FoundationLocationDto = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.FoundationLocationDto;
using InterestDto = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.InterestDto;
using LocationDto = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.LocationDto;
using ProfileImageAndBio = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.ProfileImageAndBio;
using VolunteerSkillDto = Trustesse.Ivoluntia.Commons.DTOs.OnboardingDto.VolunteerSkillDto;

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
                    var bioResponse = await UpdateBioData(volunteerOnboardingDto.BioData);
                    return ResponseHelper.BuildResponse(bioResponse.Message, bioResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 2:
                    var locationResponse = await UpdateLocation(volunteerOnboardingDto.LocationDto);
                    return ResponseHelper.BuildResponse(locationResponse.Message, locationResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 3:
                    var interestResponse = await UpdateUserInterest(volunteerOnboardingDto.Interest);
                    return ResponseHelper.BuildResponse(interestResponse.Message, interestResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 4:
                    var skillResponse = await UpdateUserSkill(volunteerOnboardingDto.Skill);
                    return ResponseHelper.BuildResponse(skillResponse.Message, skillResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 5:
                    var imageUrl = await _fileUploadService.UploadFilesAsync(volunteerOnboardingDto.ProfileAndBioData.ProfileImage);
                    var user = await _uow.userRepo.GetByExpressionAsync(u => u.Email == _currentUserService.GetUserEmail());
                    user.UserImage = imageUrl.Data[0];
                    user.Bio = volunteerOnboardingDto.ProfileAndBioData.Bio;
                    await _userManager.UpdateAsync(user);
                    var onboardingResponse = await UpdateOnBoardingProgress(user.Id,5, true);
                    hasCompleteOnboarding = true;
                    return ResponseHelper.BuildResponse(onboardingResponse.Message, onboardingResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                default:
                    return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), false);
            }
        }
        public async Task<GlobalRequestReponse<OnboardingResponseDto>>CreateOrganizationOnboarding(OrganizationOnboardingRequestDto organizationOnboardingDto)
        {
            int pageRemaining = 5 - organizationOnboardingDto.MetaData.CurrentPage;
            bool hasCompleteOnboarding = false;
            switch (organizationOnboardingDto.MetaData.CurrentPage)
            {
                case 1:
                    var mapOrganization = _mapper.Map<Foundation>(organizationOnboardingDto.foundationBioData);
                    var foundationCheck = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    if(foundationCheck != null)
                    {
                        foundationCheck.Name = organizationOnboardingDto.foundationBioData.Name;
                        foundationCheck.Website = organizationOnboardingDto.foundationBioData.Website;
                        foundationCheck.Mission = organizationOnboardingDto.foundationBioData.Mission;
                        var categoryMap = await _uow.CategoryRepository.GetByExpressionAsync(c => c.Name == organizationOnboardingDto.foundationBioData.FoundationCategory);
                        foundationCheck.CategoryId = categoryMap.Id;
                        _uow.OrganizationRepository.Update(foundationCheck);
                        await _uow.CompleteAsync();
                        return ResponseHelper.BuildResponse<OnboardingResponseDto>("foundation updated", StatusCodes.Status200OK, null, true);
                    }
                    mapOrganization.Email = _currentUserService.GetUserEmail();
                    var category = await _uow.CategoryRepository.GetByExpressionAsync(c => c.Name == organizationOnboardingDto.foundationBioData.FoundationCategory);
                    mapOrganization.CategoryId = category.Id;
                    var foundationAdmin = await _userManager.FindByEmailAsync(_currentUserService.GetUserEmail());
                    foundationAdmin.FoundationId = mapOrganization.Id;
                    mapOrganization.Status = OrganizationStatusUpdateEnums.Pending.ToString();
                    await _uow.OrganizationRepository.AddAsync(mapOrganization);
                    await _uow.CompleteAsync();
                    var response = await _userManager.UpdateAsync(foundationAdmin);
                    var onboardingResponse = await  AddOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.BioDataPage, false, 5);
                    return ResponseHelper.BuildResponse(onboardingResponse.Message, onboardingResponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 2:
                    var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    var location = await AddLocation(organizationOnboardingDto.FoundationLocationDto, foundation.Id);
                    if (location.ResponseCode != StatusCodes.Status200OK)
                        return ResponseHelper.BuildResponse<OnboardingResponseDto>(location.Message, location.ResponseCode, null, false);
                    if (location.ResponseCode == StatusCodes.Status200OK && location.Message == "location updated")
                        return ResponseHelper.BuildResponse<OnboardingResponseDto>(location.Message, StatusCodes.Status200OK,null,true);
                    foundation.LocationId = location.Message;
                    _uow.OrganizationRepository.Update(foundation);
                    await _uow.CompleteAsync();
                    var onboardingResp = await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Location, false);
                    return ResponseHelper.BuildResponse(onboardingResp.Message, onboardingResp.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 3:
                    var foundationMap = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    var result = await AddFoundationCause(organizationOnboardingDto.CauseDto.Names, foundationMap.Id, _currentUserService.GetUserEmail());
                    return ResponseHelper.BuildResponse(result.Message, result.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 4:
                    var imageUrl = await _fileUploadService.UploadFilesAsync(organizationOnboardingDto.ProfileLogo.Logo);
                    var foundationResponse = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    if(foundationResponse.Logo != null)
                    {
                        foundationResponse.Logo = imageUrl.Data[0];
                        _uow.OrganizationRepository.Update(foundationResponse);
                        await _uow.CompleteAsync();
                        return ResponseHelper.BuildResponse<OnboardingResponseDto>("logo updated",StatusCodes.Status200OK,null,true);
                    }
                    foundationResponse.Logo = imageUrl.Data[0];
                    _uow.OrganizationRepository.Update(foundationResponse);
                    await _uow.CompleteAsync();
                    var onboardResp = await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Profile, false);
                    return ResponseHelper.BuildResponse(onboardResp.Message, onboardResp.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                case 5:
                    var foundationMapping = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
                    foundationMapping.HasAgreedToDisclaimer = organizationOnboardingDto.Disclaimer.HasAgreedToDisclaimer;
                    foundationMapping.HasAgreedToDisclaimer = organizationOnboardingDto.Disclaimer.HasAgreedToDisclaimer;
                    foundationMapping.IsActive = true;
                    _uow.OrganizationRepository.Update(foundationMapping);
                    await _uow.CompleteAsync();
                    var onBoardingesponse = await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Disclaimer, true);
                    hasCompleteOnboarding = true;
                    return ResponseHelper.BuildResponse(onBoardingesponse.Message, onBoardingesponse.StatusCode, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), true);
                default:
                    return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, OnboardingResponseDto.BuildOnboardingResponseDto(pageRemaining, hasCompleteOnboarding), false);
            }
        }

        public async Task<ApiResponse<string>> UpdateBioData(BioData model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionIncludeAsync(u => u.Id == _currentUserService.GetUserId(), u => u.OnboardingProgress);
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            volunteer.FirstName = model.FirstName;
            volunteer.LastName = model.LastName;
            volunteer.Gender = model.Gender;
            volunteer.OtherName = model.OtherName;  
            volunteer.DateOfBirth = model.DateOfBirth;
            var bioUpdate = await _userManager.UpdateAsync(volunteer);
            if (bioUpdate.Succeeded)
            {
                if(volunteer.OnboardingProgress == null)
                    await AddOnBoardingProgress(volunteer.Id, 1, false, 5);
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
            var user = await _uow.userRepo.GetByExpressionIncludeAsync(u => u.Id == _currentUserService.GetUserId(), u => u.OnboardingProgress);
            var locationUpdate = await _uow.locationRepo.GetByExpressionAsync(l => l.UserId == _currentUserService.GetUserId());
            if(locationUpdate != null)
            {
                //update the location

                locationUpdate.CountryId = country.Id;
                locationUpdate.StateId = state.Id;
                locationUpdate.City = model.City;
                locationUpdate.Zipcode = model.ZipCode;
                locationUpdate.Address = model.Address;
                _uow.locationRepo.Update(locationUpdate);
                await _uow.CompleteAsync();
                return ApiResponse<string>.Success("Volunteer Location updated successfully.", null);
            }
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
            return ApiResponse<string>.Success("Volunteer Location added successfully.", null);
        }

        public async Task<ApiResponse<string>> UpdateUserInterest(InterestDto model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Volunteer not found.");
            }
            var userInterests = await _uow.userInterestLinkRepo.GetByExpressionAsync(ul => ul.UserId == _currentUserService.GetUserId());   
            if(userInterests != null)
            {
                if (model.Names.Any())
                {
                    foreach (var name in model.Names)
                    {
                        var interestExist = await _uow.InterestRepository.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                        var userInterest = await _uow.userInterestLinkRepo.GetByExpressionAsync(ui => ui.UserId == _currentUserService.GetUserId() && ui.InterestId == interestExist.Id);
                        if (userInterest == null)
                        {
                            var saveUserInterest = new UserInterestLink()
                            {
                                UserId = volunteer.Id,
                                InterestId = interestExist.Id
                            };
                            await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                        }
                    }
                    await _uow.CompleteAsync();  
                }
                return ApiResponse<string>.Success("interest updated", null);
            }
            if (model.Names.Any())
            {
                foreach (var name in model.Names)
                {
                    var interestExist = await _uow.InterestRepository.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                    var saveUserInterest = new UserInterestLink()
                    {
                        UserId = volunteer.Id,
                        InterestId = interestExist.Id
                    };
                    await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                }
                await _uow.CompleteAsync();
            }
            var response = await UpdateOnBoardingProgress(volunteer.Id, 3, false);
            if (response.StatusCode == StatusCodes.Status200OK)
                return ApiResponse<string>.Success("onboarding updated", response.Data);
            return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest,"unable to update onboarding.");
        }

        public async Task<ApiResponse<string>> UpdateUserSkill(VolunteerSkillDto model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            var userSkill = await _uow.userSkillLinkRepo.GetByExpressionAsync(us => us.UserId == _currentUserService.GetUserId());
            if (userSkill != null)
            {
                if (model.Names.Any())
                {
                    foreach (var name in model.Names)
                    {
                        var skill = await _uow.skillRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                        var userSkillLink = await _uow.userSkillLinkRepo.GetByExpressionAsync(us => us.UserId == _currentUserService.GetUserId() && us.SkillId == skill.Id);
                        if (userSkillLink == null)
                        {
                            var userSKillMap = new UserSkillLink()
                            {
                                UserId = volunteer.Id,
                                SkillId = skill.Id
                            };
                            await _uow.userSkillLinkRepo.AddAsync(userSKillMap);
                        }
                    }
                    await _uow.CompleteAsync();
                }
                return ApiResponse<string>.Success("skill updated", null);
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
            return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest,"unable to update onboarding.");
        }

        public async Task<ApiResponse<string>> UpdateProfileImageAndBio(ProfileImageAndBio model)
        {
            var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == _currentUserService.GetUserId());
            if (volunteer == null)
            {
                return ApiResponse<string>.Failure(404, "Volunteer not found.");
            }
            if(volunteer.Bio != null)
            {
                //update it
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
                    return ApiResponse<string>.Success("OnboardingProgress has been updated successfully.", null);
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "something went wrong");
            }
            return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, "onboarding does not exit");
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
            var foundationCause = await _uow.CauseFoundationRepository.GetByExpressionAsync(cf =>cf.FoundationId == foundationId);
            if (foundationCause != null)
            {
                if (causeName.Any())
                {
                    foreach (var cause in causeName)
                    {
                        var dbCause = await _uow.CauseRepository.GetByExpressionAsync(x => x.Name.ToLower() == cause.ToLower());
                        var dbFoundationCause = await _uow.CauseFoundationRepository.GetByExpressionAsync(cf => cf.FoundationId == foundationId && cf.CauseId == dbCause.Id);
                        if (dbFoundationCause == null)
                        {
                            var foundationCauseMap = new FoundationCauses()
                            {
                                CauseId = dbCause.Id,
                                FoundationId = foundationId,
                                CreatedBy = foundationAdminEmail
                            };
                            await _uow.CauseFoundationRepository.AddAsync(foundationCauseMap);
                        }
                    }
                    var response = await _uow.CompleteAsync();
                    if(response > 0)
                        return ApiResponse<string>.Success("cause updated", null);
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "something went wrong");
                }
            }
            var foundationCauses = causes.Select(cause => new FoundationCauses
            {
                CauseId = cause.Id,
                FoundationId = foundationId,
                CreatedBy = foundationAdminEmail
            }).ToList();
            await _uow.CauseFoundationRepository.AddManyAsync(foundationCauses);
            var responsedb = await _uow.CompleteAsync();
            if(responsedb > 0)
            {
                var onboardingUpdateResponse = await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Cause, false);
                if(onboardingUpdateResponse.StatusCode == StatusCodes.Status200OK)
                    return ApiResponse<string>.Success("foundation causes added successfully", "foundation cause added");
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "something went wrong");
            }
            return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "something went wrong");
        }
        public async Task<GlobalRequestReponse<string>> AddLocation(FoundationLocationDto foundationLocationDto, string foundationId)
        {
            var country = await _uow.countryRepo.GetByExpressionAsync(c => c.CountryName == foundationLocationDto.FoundationCountry);
            if (country == null)
                return ResponseHelper.BuildResponse<string>("Country doesn't exist", StatusCodes.Status404NotFound, null, false);
            var state = await _uow.stateRepo.GetByExpressionAsync(s => s.StateName == foundationLocationDto.FoundationState);
            if (state == null)
                return ResponseHelper.BuildResponse<string>("state doesn't exist", StatusCodes.Status404NotFound, null, false);
            var locationCheck = await _uow.locationRepo.GetByExpressionAsync(l => l.UserId == _currentUserService.GetUserId());
            if (locationCheck != null)
            {
                locationCheck.Address = foundationLocationDto.Address;  
                locationCheck.City = foundationLocationDto.City;        
                locationCheck.Zipcode = foundationLocationDto.Zipcode;
                locationCheck.Country.CountryName = foundationLocationDto.FoundationCountry;
                locationCheck.State.StateName = foundationLocationDto.FoundationState;
                locationCheck.CountryId = country.Id;
                locationCheck.StateId = state.Id;   
                locationCheck.FoundationId = foundationId;  
                locationCheck.UserId = _currentUserService.GetUserId(); 
                _uow.locationRepo.Update(locationCheck);
                await _uow.CompleteAsync();
                return ResponseHelper.BuildResponse<string>("location updated", StatusCodes.Status200OK, null, true); 
            }
            var mapLocation = _mapper.Map<Location>(foundationLocationDto);
            mapLocation.CountryId = country.Id;
            mapLocation.StateId = state.Id;
            mapLocation.FoundationId = foundationId;
            mapLocation.UserId = _currentUserService.GetUserId();
            await _uow.locationRepo.AddAsync(mapLocation);
            var rowchange = await _uow.CompleteAsync();
            return ResponseHelper.BuildResponse<string>(mapLocation.Id, StatusCodes.Status200OK, null, true); 
        }
    }
}
