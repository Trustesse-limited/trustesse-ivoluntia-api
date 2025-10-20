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
using Skill = Trustesse.Ivoluntia.Commons.DTOs.SkillDto;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthService : IAuthService   
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otp;
    private readonly INotificationService _notify;
    private readonly IEmailService _email;
    public AuthService(IUnitOfWork uow, IMapper mapper, UserManager<User> userManager, IOtpService otp,
        INotificationService notify, IEmailService email)
    {
            _uow = uow; 
            _mapper = mapper;   
            _userManager = userManager; 
            _otp = otp;
            _notify = notify;
            _email = email;
    }
    public async Task<ApiResponse<string>> CreateVolunteer(VolunteerSignUpDto model)
    {
            User volunteer = null;
            
            if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                model.MetaData.CurrentPage == (int)OnBoardingPages.AuthInfoPage)
            {
            //Check if the Volunteer already exist
            var VolunteerExists = await _uow.userRepo.GetByExpressionAsync(x =>
                x.Email == model.AuthInfo.Email);
            if (VolunteerExists != null)
                return ApiResponse<string>.Failure(409, $"Volunteer with Email -> {model.AuthInfo.Email}  already exist.");
            volunteer = _mapper.Map<User>(model);
                volunteer.UserName = model.AuthInfo.Email;
                volunteer.Email = model.AuthInfo.Email.Trim();
                volunteer.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
                volunteer.IsActive = false;
                volunteer.HasAgreedToTermsAndCondition = model.AuthInfo.HasAcceptedTOC;
                var result = await _userManager.CreateAsync(volunteer, model.AuthInfo.Password.Trim());
                if (result.Succeeded)
                {
                    //Generate OTP
                var otp = await _otp.GenerateOtpAsync(volunteer.Id, OtpPurpose.Signup);//GenerateOTP();
                    volunteer.OTP = otp;
                    volunteer.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                    volunteer.Id = volunteer.Id;
                    result = await _userManager.UpdateAsync(volunteer).ConfigureAwait(false);
                    if (!result.Succeeded)
                    {
                        return ApiResponse<string>.Failure(500, "Unable to update user account with OTP details.");
                    }
                    else
                    {
                    // send otp to email Address
                    var dict = new Dictionary<string, string>()
                    {
                        {"firstname", volunteer.FirstName },
                        { "otp", volunteer.OTP}
                    };
                    var sendMail = await _notify.ComposeNotificationAsync("OtpCode", "Email", dict);
                    if(sendMail != null)
                    {
                        var msg = new EmailModel
                        {
                            Receivers = new List<string> { volunteer.Email},
                            Attachments = null,
                            Subject = "OTP",
                            Message = sendMail.Data
                        };
                        await _email.SendEmailASync(msg);
                    }
                    }
                    

                    var obj = new OnboardingProgress();
                {
                    obj.UserId = volunteer.Id;
                    obj.TotalPages = 6;
                    obj.LastCompletedPage = model.MetaData.CurrentPage;
                    obj.HasCompletedOnboarding = false;
                }
                await _uow.onboardingProgressRepo.AddAsync(obj);
                await _uow.CompleteAsync();
                }
            }
            else if(model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                    model.MetaData.CurrentPage == (int)OnBoardingPages.BioDataPage)
            {
                await UpdateBioData(model.BioData);
            }
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Location)
            {
            await UpdateLocation(model.LocationDto);
            }
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Interest)
            {
                await UpdateUserInterest(model.Interest);
            }
            
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.Skill)
            {
                await UpdateUserSkill(model.Skill);
            }
            
            else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
                     model.MetaData.CurrentPage == (int)OnBoardingPages.ProfileImageAndBio)
            {
                await UpdateProfileImageAndBio(model.ProfileAndBioData);
            }
            return ApiResponse<string>.Success("Volunteer successfully SignUp.", null);
    }
    public async Task<ApiResponse<string>> UpdateBioData(BioData model)
    {
        var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == model.UserId);
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
                await UpdateOnBoardingProgress(volunteer.Id, 2, false);
            }
        return ApiResponse<string>.Success("Volunteer BioData updated successfully.", null);
    }
    public async Task<ApiResponse<string>> UpdateLocation(LocationDto model)
    {
        var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == model.UserId);
        if (volunteer != null)
        {
            return ApiResponse<string>.Failure(404, "Volunteer not found.");
        }
            var country = await _uow.countryRepo.GetByIdAsync(Guid.Parse(model.CountryId));
            if (country == null)
                return ApiResponse<string>.Failure(404, $"Country doesn't exist.");
            var state = await _uow.stateRepo.GetByIdAsync(Guid.Parse(model.StateId));
            if (state == null)
                return ApiResponse<string>.Failure(404, $"State doesn't exist.");
            var location = new Trustesse.Ivoluntia.Domain.Entities.Location
            {
                CountryId = Guid.Parse(model.CountryId),
                StateId = Guid.Parse(model.StateId),
                City = model.City,
                Zipcode = model.ZipCode,
                Address = model.Address,
                UserId = volunteer.Id
            };
            await _uow.locationRepo.AddAsync(location);
            var rowchange = await _uow.CompleteAsync();
            if (rowchange > 0)
            {
                await UpdateOnBoardingProgress(volunteer.Id, 3, false);
            }
        
        return ApiResponse<string>.Success("Volunteer Location updated successfully.", null);
    }
    public async Task<ApiResponse<string>> UpdateUserInterest(InterestDto model)
    {
        var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == model.UserId);
        if (volunteer != null)
        {
            return ApiResponse<string>.Failure(404, "Volunteer not found.");
        }
            if (model.Names.Any())
            {
                foreach (var name in model.Names)
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
            await UpdateOnBoardingProgress(volunteer.Id, 4, false);
        
        return ApiResponse<string>.Success("Volunteer Interest updated successfully.", null);
    }
    public async Task<ApiResponse<string>> UpdateUserSkill(SkillDto model)
    {
        var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == model.UserId);
        if (volunteer != null)
        {
            return ApiResponse<string>.Failure(404, "Volunteer not found.");
        }
            if (model.Names.Any())
            {
                foreach (var name in model.Names)
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
                        var saveUserSkill = new UserSkillLink()
                        {
                            UserId = volunteer.Id,
                            SkillId = skillExist.Id
                        };
                        await _uow.userSkillLinkRepo.AddAsync(saveUserSkill);
                        await _uow.CompleteAsync();
                    }
                }
            }
            await UpdateOnBoardingProgress(volunteer.Id, 5, false);
        return ApiResponse<string>.Success("Volunteer Skills updated successfully.", null);
    }
    public async Task<ApiResponse<string>> UpdateProfileImageAndBio(ProfileImageAndBio model)
    {
        var volunteer = await _uow.userRepo.GetByExpressionAsync(x => x.Id == model.UserId);
        if (volunteer == null)
        {
            return ApiResponse<string>.Failure(404, "Volunteer not found.");
        }
            volunteer.UserImage = model.ProfileImageurl;
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
   
    public async Task<ApiResponse<string>> ResetPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
        if (user != null)
        {
            //Generate OTP
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.PasswordReset);
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
           var  result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                return ApiResponse<string>.Failure(500, "Unable to update user account with OTP details.");
            }
            else
            {
                // send otp to email Address
                var dict = new Dictionary<string, string>()
                    {
                        {"firstname", user.FirstName },
                        { "otp", user.OTP}
                    };
                var sendMail = await _notify.ComposeNotificationAsync("Otp", "Email", dict);
                if (sendMail != null)
                {
                    var msg = new EmailModel
                    {
                        Receivers = new List<string> { user.Email },
                        Attachments = null,
                        Subject = "OTP",
                        Message = sendMail.Data
                    };
                    await _email.SendEmailASync(msg);
                    return ApiResponse<string>.Success("An OTP has been sent to your registered email", null);
                }
            }
        }
        return ApiResponse<string>.Failure(400, "User not found.");
    }
    public async Task<ApiResponse<string>> CreatePasswordAsync(ResetPasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
        if (user == null)
        {
            return ApiResponse<string>.Failure(400, "User not found.");
        }
        var result = await ForgetPasswordAsync(user, model.NewPassword);
        if (result.Succeeded)
        {
            return ApiResponse<string>.Success("New Password successfully Created", null);
        }
        return ApiResponse<string>.Failure(400, "unable to get hash password");
    }
    public async Task<ApiResponse<string>> ValidateOTP(string otp, string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
        if (user == null)
        {
            return ApiResponse<string>.Failure(404, "User not found.");
        }
        TimeSpan time = DateTime.Now.TimeOfDay.Subtract(user.OtpSubmittedTime.GetValueOrDefault().TimeOfDay);
        if (user.OTP == otp && time.TotalSeconds <= 600)
        {
            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return ApiResponse<string>.Success("Account created Successfully", null);
            }
            return ApiResponse<string>.Failure(500, "Faill to update user information");
        }
        return ApiResponse<string>.Failure(400, "Wrong OTP. Please provide the OTP sent to your email address");
       
    }
    public async Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
        if (user == null)
        {
            return ApiResponse<string>.Failure(400, "User not found.");
        }
        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword.Trim(), model.NewPassword.Trim());
        if(result.Succeeded)
        {
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return ApiResponse<string>.Success("Password Successfully Changed", null);
        }
        else
        {
            return ApiResponse<string>.Failure(400, "Password change Fail");
        }   
    }
    public async Task<ApiResponse<string>> ResendOTP(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
        if (user != null)
        {
            //Generate OTP
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.PasswordReset);
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
            if (!result.Succeeded)
            {
                return ApiResponse<string>.Failure(500, "Unable to update user account with OTP details.");
            }
            else
            {
                // send otp to email Address
                var dict = new Dictionary<string, string>()
                    {
                        {"firstname", user.FirstName },
                        { "otp", user.OTP}
                    };
                var sendMail = await _notify.ComposeNotificationAsync("Otp", "Email", dict);
                if (sendMail != null)
                {
                    var msg = new EmailModel
                    {
                        Receivers = new List<string> { user.Email },
                        Attachments = null,
                        Subject = "OTP",
                        Message = sendMail.Data
                    };
                    await _email.SendEmailASync(msg);
                    return ApiResponse<string>.Success("An OTP has been sent to your registered email", null);
                }
            }
        }
        return ApiResponse<string>.Failure(400, "User not found.");
    }
    public async Task<IdentityResult> ForgetPasswordAsync(User user, string password)
    {
        IdentityResult result = null;
        IdentityErrorDescriber errorDescriber = new IdentityErrorDescriber();
        var passwordHash = _userManager.PasswordHasher.HashPassword(user, password);
        if (passwordHash != null)
        {
            user.PasswordHash = passwordHash;
            result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
        }
        else 
        {
            return IdentityResult.Failed(errorDescriber.DefaultError());
        }
        return result;
    }
    private string GenerateOTP()
    {
        try
        {
            byte[] seed = Guid.NewGuid().ToByteArray();
            Random _random = new Random(BitConverter.ToInt32(seed, 0));
            int _rand = _random.Next(100000, 1000000);

            return _rand.ToString();
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}