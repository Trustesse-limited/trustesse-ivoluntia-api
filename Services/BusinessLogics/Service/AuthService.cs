using System.Net;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Location = Trustesse.Ivoluntia.Commons.DTOs.LocationDto;
using Skill = Trustesse.Ivoluntia.Commons.DTOs.Skill;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthService : IAuthService   
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    public AuthService(IUnitOfWork uow, IMapper mapper, UserManager<User> userManager)
    {
            _uow = uow; 
            _mapper = mapper;   
            _userManager = userManager; 
    }

    public async Task<CustomResponse> CreateVolunteer(VolunteerSignUpDto model)
    {
        CustomResponse response = null;
        User volunteer = null;
        try
        {
            //Check if the Volunteer already exist
            var VolunteerExists = await _uow.userRepo.GetByExpressionAsync(x => 
                x.Email == model.AuthInfo.Email);
            if (VolunteerExists != null)
                return new CustomResponse((int)HttpStatusCode.Conflict, $"Volunteer with Email -> {model.AuthInfo.Email}  already exist.", null);
            if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                model.MetaData.CurrentPage == (int)OnBoardingPages.AuthInfoPage)
            {
                volunteer = _mapper.Map<User>(model);
                volunteer.UserName = model.AuthInfo.Email;
                volunteer.Email = !string.IsNullOrWhiteSpace(model.AuthInfo.Email.Trim()) ? model.AuthInfo.Email.Trim() : default;
                volunteer.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                volunteer.IsActive = false;
                volunteer.HasAgreedToTermsAndCondition = model.AuthInfo.HasAcceptedTOC;
                
                var result = await _userManager.CreateAsync(volunteer, model.AuthInfo.Password.Trim());
                if (result.Succeeded)
                {
                    var otp = GenerateOTP();
                    volunteer.OTP = otp;
                    volunteer.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                    volunteer.Id = volunteer.Id;
                    result = await _userManager.UpdateAsync(volunteer).ConfigureAwait(false);

                    if (!result.Succeeded)
                    {
                        response = new CustomResponse(500, "Unable to update user account with OTP details.");
                    }
                    // emailServices
                    await UpdateOnBoardingProgress(volunteer.Id, 1, false);
                }
            }
            else if(model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                    model.MetaData.CurrentPage == (int)OnBoardingPages.BioDataPage)
            {
               volunteer.FirstName = model.BioData.FirstName;
               volunteer.LastName = model.BioData.LastName;
               volunteer.Gender = model.BioData.Gender;
               volunteer.DateOfBirth = model.BioData.DateOfBirth;
               var bioUpdate = await _userManager.UpdateAsync(volunteer);
               if (bioUpdate.Succeeded)
                   await UpdateOnBoardingProgress(volunteer.Id, 2, false);
            }
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Location)
            {
                var country = await _uow.countryRepo.GetByIdAsync(model.LocationDto.CountryId);
                if (country == null)
                    return new CustomResponse((int)HttpStatusCode.NotFound, $"Country doesn't exist.", null);
                var state = await _uow.stateRepo.GetByIdAsync(model.LocationDto.StateId);
                if (state == null)
                    return new CustomResponse((int)HttpStatusCode.NotFound, $"State doesn't exist.", null);
                var location = new Trustesse.Ivoluntia.Domain.Entities.Location
                {
                    CountryId = model.LocationDto.CountryId,
                    StateId = model.LocationDto.StateId,
                    City = model.LocationDto.City,
                    Zipcode = model.LocationDto.ZipCode,
                    Address = model.LocationDto.Address,
                };
                await _uow.locationRepo.AddAsync(location);
                var rowchange = await _uow.CompleteAsync();
                if (rowchange > 0)
                {
                    UpdateOnBoardingProgress(volunteer.Id, 3, false);
                }
            }
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Interest)
            {
                if (model.Interest.Names.Any())
                {
                    foreach (var name in model.Interest.Names)
                    {
                        var interestExist  = await _uow.interestRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                        if (interestExist == null)
                        {
                            var createInterest = new Interest()
                            {
                                Name = name
                            };
                            await _uow.interestRepo.AddAsync(createInterest);
                            if (await _uow.CompleteAsync() > 0)
                            {
                                var saveUserInterest = new UserInterestLink()
                                {
                                    UserId = volunteer.Id,
                                    InterestId = createInterest.Id
                                };
                                await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                                await _uow.CompleteAsync();
                            }
                        }
                        else
                        {
                            var saveUserInterest = new UserInterestLink()
                            {
                                UserId = volunteer.Id,
                                InterestId = interestExist.Id
                            };
                            await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                            await _uow.CompleteAsync();
                        }
                    }
                }
                UpdateOnBoardingProgress(volunteer.Id, 4, false);
            }
            
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Skill)
            {
                if (model.Skill.Names.Any())
                {
                    foreach (var name in model.Skill.Names)
                    {
                        var skillExist  = await _uow.skillRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
                        if (skillExist == null)
                        {
                            var createSkill = new Trustesse.Ivoluntia.Domain.Entities.Skill()
                            {
                                Name = name
                            };
                            await _uow.skillRepo.AddAsync(createSkill);
                            if (await _uow.CompleteAsync() > 0)
                            {
                                var saveUserSkill = new UserSkillLink()
                                {
                                    UserId = volunteer.Id,
                                    SkillId = createSkill.Id
                                };
                                await _uow.userSkillLinkRepo.AddAsync(saveUserSkill);
                                await _uow.CompleteAsync();
                            }
                        }
                        else
                        {
                            var saveUserInterest = new UserInterestLink()
                            {
                                UserId = volunteer.Id,
                                InterestId = skillExist.Id
                            };
                            await _uow.userInterestLinkRepo.AddAsync(saveUserInterest);
                            await _uow.CompleteAsync();
                        }
                    }
                }
                UpdateOnBoardingProgress(volunteer.Id, 5, false);
            }
            
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.ProfileImageAndBio)
            {
                volunteer.UserImage = model.ProfileAndBioData.ProfileImageurl;
                volunteer.Bio = model.ProfileAndBioData.Bio;
                var profile = await _userManager.UpdateAsync(volunteer);
                if (profile.Succeeded)
                {
                    UpdateOnBoardingProgress(volunteer.Id, 6, true);
                }


            }
                
        }
        catch (Exception ex)
        {
          
        }
        return response;
    }

    public async Task<CustomResponse> UpdateOnBoardingProgress(string userId, int lastCompletedPage, bool hasCompletedOnboarding)
    {
        CustomResponse response = null;
        try
        {
            var updateProgressTable = await _uow.onboardingProgressRepo.GetByIdAsync(userId);
            if (updateProgressTable != null)
            {
                var obj = new OnboardingProgress();
                {
                    obj.LastCompletedPage = lastCompletedPage;
                    obj.HasCompletedOnboarding = hasCompletedOnboarding;
                }
                await _uow.onboardingProgressRepo.UpdateAsync(obj);
                if (await _uow.CompleteAsync() > 0)
                {
                    response = new CustomResponse(200, "OnboardingProgress has been updated successfully.");
                }
            }
        }
        catch (Exception ex)
        {
            
        }
        return response;
        
    }
    
    private string GenerateOTP()
    {
        try
        {
            byte[] seed = Guid.NewGuid().ToByteArray();
            Random _random = new Random(BitConverter.ToInt32(seed, 0));
            int _rand = _random.Next(1000, 10000);

            return _rand.ToString();
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}