using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;
using System.Web;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using static System.Net.WebRequestMethods;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otp;
    private readonly INotificationService _notify;
    private readonly IEmailService _email;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICurrentUserService _currentUserService;
    public AuthenticationService(IUnitOfWork uow,
        IMapper mapper,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger,
        IOtpService otp,
        INotificationService notify,
        IEmailService email,
        IUserRepository userRepository,
        IFileUploadService fileUploadService,
        ICurrentUserService currentUserService)
    {
        _uow = uow;
        _mapper = mapper;
        _otp = otp;
        _notify = notify;
        _email = email;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _logger = logger;
        _fileUploadService = fileUploadService;
        _currentUserService = currentUserService;
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
                var otp = GenerateOTP();
                volunteer.OTP = otp;
                volunteer.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
                volunteer.Id = volunteer.Id;
                result = await _userManager.UpdateAsync(volunteer).ConfigureAwait(false);
                if (!result.Succeeded)
                {
                    return ApiResponse<string>.Failure(500, "Unable to update user account with OTP details.");
                }
                // emailServices
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
        else if (model.MetaData.AccountType.ToLower() == AccountType.Volunteer.ToString().ToLower() &&
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
                var interestExist = await _uow.interestRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
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
                var skillExist = await _uow.skillRepo.GetByExpressionAsync(x => x.Name.ToLower() == name.ToLower());
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
    public async Task<ApiResponse<string>> AddFoundationCause(List<string> causeName,string foundationId, string foundationAdminEmail)
    {
            var causes = await _uow.CauseRepository
                .GetAsync(c => causeName.Contains(c.Name));
            if (causes == null || !causes.Any())
            {
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest,"No matching causes found");
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
            return($"Country doesn't exist.");
        var state = await _uow.stateRepo.GetByExpressionAsync(s => s.StateName == foundationLocationDto.FoundationState);
        if (state == null)
            return("State doesn't exist.");
        foundationLocationDto.StateId = state.Id;
        foundationLocationDto.CountryId = country.Id;
        var mapLocation = _mapper.Map<Location>(foundationLocationDto);
        mapLocation.FoundationId = foundationId;    
        await _uow.locationRepo.AddAsync(mapLocation);
        var rowchange = await _uow.CompleteAsync();
        return mapLocation.Id;
    }

    public async Task<GlobalRequestReponse<string>> CreateOrganization(CreateFoundationRequestDto createFoundationRequestDto)
    {
        if (createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.AuthInfoPage)
        {
            var mapFoundationAdmin = _mapper.Map<User>(createFoundationRequestDto.FoundationAdminInfo);
            var foundationAdminCheck = await _uow.userRepo.GetByExpressionAsync(x =>
            x.Email == createFoundationRequestDto.FoundationAdminInfo.Email);
            if (foundationAdminCheck != null)
                return ResponseHelper.BuildResponse("user already exist", StatusCodes.Status400BadRequest, "user exist", false);
            mapFoundationAdmin.UserName = createFoundationRequestDto.FoundationAdminInfo.Email;
            mapFoundationAdmin.PasswordHash = createFoundationRequestDto.FoundationAdminInfo.Password;
            mapFoundationAdmin.Email = createFoundationRequestDto.FoundationAdminInfo.Email.Trim();
            mapFoundationAdmin.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            mapFoundationAdmin.IsActive = false;
            mapFoundationAdmin.HasAgreedToTermsAndCondition = createFoundationRequestDto.FoundationAdminInfo.HasAgreedToTermsAndCondition;
            mapFoundationAdmin.OTP = GenerateOTP();
            mapFoundationAdmin.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.CreateAsync(mapFoundationAdmin, createFoundationRequestDto.FoundationAdminInfo.Password.Trim());
            await _userManager.AddToRoleAsync(mapFoundationAdmin, UserRolesEnum.FoundationAdmin.ToString());
            if (result.Succeeded)
            {
                // emailService 
                var dictionary = new Dictionary<string, string>()
             {
                {"firstname", mapFoundationAdmin.Email},
                { "otp", mapFoundationAdmin.OTP}
             };
                var notificationTemplate = await _notify.ComposeNotificationAsync(NotificationTypeEnum.OtpRequest.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
                if (notificationTemplate != null)
                {
                    var message = new EmailModel
                    {
                        Receivers = new List<string> {mapFoundationAdmin.Email},
                        Subject = "OTP",
                        Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
                    };
                    var emailResponse = await _email.SendEmailASync(message);
                }
                var otpDto = _mapper.Map<OtpDto>(mapFoundationAdmin);
                otpDto.IsUsed = false;
                otpDto.Purpose = OtpPurpose.Signup.ToString();
                otpDto.OtpCode = mapFoundationAdmin.OTP;
                otpDto.Channel = NotificationChannelEnum.Email.ToString();
                otpDto.UserId = mapFoundationAdmin.Id;
                var otp = _mapper.Map<Otp>(otpDto);
                await _uow.OtpRepo.AddAsync(otp);
                var response = await AddOnBoardingProgress(mapFoundationAdmin.Id, createFoundationRequestDto.MetaData.CurrentPage,false,6);
                if(response.StatusCode == StatusCodes.Status200OK)
                    return ResponseHelper.BuildResponse("foundation admin created", StatusCodes.Status200OK, "created successfully", true);
                return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);
            }
        }
        else if (createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.BioDataPage)
        {
            var mapOrganization = _mapper.Map<Foundation>(createFoundationRequestDto.foundationBioData);
            mapOrganization.Email = _currentUserService.GetUserEmail();
            var category = await _uow.CategoryRepository.GetByExpressionAsync(c => c.Name == createFoundationRequestDto.foundationBioData.FoundationCategory);
            mapOrganization.CategoryId = category.Id;
            var foundationAdmin = await _userManager.FindByEmailAsync(_currentUserService.GetUserEmail());
            foundationAdmin.FoundationId = mapOrganization.Id;
            await _uow.OrganizationRepository.AddAsync(mapOrganization);
            await _uow.CompleteAsync();
            var response = await _userManager.UpdateAsync(foundationAdmin);
            if(response.Succeeded)
                return ResponseHelper.BuildResponse("foundation created", StatusCodes.Status200OK, "created successfully", true);
            return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);
        }
        else if (createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.Location)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            createFoundationRequestDto.FoundationLocationDto.UserId = _currentUserService.GetUserId();
            var locationId = await AddLocation(createFoundationRequestDto.FoundationLocationDto, foundation.Id);
            foundation.LocationId = locationId;    
            _uow.OrganizationRepository.Update(foundation);
            var response = await _uow.CompleteAsync();
            if(response > 0)
                return ResponseHelper.BuildResponse("foundation location added", StatusCodes.Status200OK, "created successfully", true);
            return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);

        }
        else if (createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.Cause)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            var response = await AddFoundationCause(createFoundationRequestDto.CauseDto.Names, foundation.Id, _currentUserService.GetUserEmail()); 
            if(response.StatusCode == StatusCodes.Status200OK)
                return ResponseHelper.BuildResponse("foundation cause added", StatusCodes.Status200OK, "created successfully", true);
            return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);
        }

        else if (createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.Profile)
        {
            var imageUrl = await _fileUploadService.UploadFilesAsync(createFoundationRequestDto.ProfileLogo.Logo);
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            foundation.Logo = imageUrl.Data[0];
            _uow.OrganizationRepository.Update(foundation);
            var response = await _uow.CompleteAsync();
            if(response > 0)
                return ResponseHelper.BuildResponse("foundation logo added", StatusCodes.Status200OK, "created successfully", true);
            return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);

        }

        else  if(createFoundationRequestDto.MetaData.CurrentPage == (int)OrganizationOnboardingEnum.Disclaimer)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            foundation.HasAgreedToDisclaimer = createFoundationRequestDto.Disclaimer.HasAgreedToDisclaimer;
            foundation.HasAgreedToDisclaimer = createFoundationRequestDto.Disclaimer.HasAgreedToDisclaimer;
            foundation.IsActive = true;
            _uow.OrganizationRepository.Update(foundation);
            await _uow.CompleteAsync();
            var response = await UpdateOnBoardingProgress(_currentUserService.GetUserId(), createFoundationRequestDto.MetaData.CurrentPage, true);
            if(response.StatusCode == StatusCodes.Status200OK)
                return ResponseHelper.BuildResponse("foundation disclaimer updated", StatusCodes.Status200OK, "created successfully", true);
            return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);
        }
        return ResponseHelper.BuildResponse("unexpected result", StatusCodes.Status400BadRequest, "not successful", false);
    }

    public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailWithFoundationAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LoginResponseModel>.Failure(401, "Invalid credentials");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Volunteer";
        var isSuperAdmin = primaryRole == UserRolesEnum.SuperAdmin.ToString();

        if (!user.IsActive || (!isSuperAdmin && user.Foundation?.IsActive != true))
        {
            return ApiResponse<LoginResponseModel>.Failure(401, "Account is inactive");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return ApiResponse<LoginResponseModel>.Failure(403, "Account is locked for 1 hour due to multiple failed login attempts.");
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return ApiResponse<LoginResponseModel>.Failure(401, "Invalid credentials");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        user.LastLogin = DateTime.UtcNow;
        user.DateUpdated = DateTime.UtcNow;

        var jwtClaims = new JwtClaimsModel
        {
            UserId = user.Id,
            Email = user.Email,
            Role = primaryRole,
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationName = user?.Foundation?.Name ?? string.Empty,
            FoundationId = user?.FoundationId ?? string.Empty  
        };

        var accessToken = _jwtTokenService.GenerateAccessTokenAsync(jwtClaims, primaryRole);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(
            user?.Id!, primaryRole);

        user!.LastLogin = DateTime.UtcNow;
        await _uow.CompleteAsync();

        var longinResponse = new LoginResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? true,
            LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0,
            Message = "Login successful"
        };

        return ApiResponse<LoginResponseModel>.Success("Successfully logged in", longinResponse);
    }

    public async Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken)
    {
        var validation = await _jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken, request.UserId);

        if (!validation.IsValid)
        {
            _logger.LogWarning("Invalid refresh token used. Status: {Status}, Error: {Error}",
                validation.Status, validation.ValidationError);

            return ApiResponse<RefreshTokenResponseModel>.Failure(400, $"Invalid refresh token due to {nameof(validation.Status)}");
        }


        var user = await _userRepository.GetUserByEmailWithFoundationAsync(request.UserId, cancellationToken);

        if (user is null)
        {
            return ApiResponse<RefreshTokenResponseModel>.Failure(404, "User not found");
        }

        var userRoles = await _userManager.GetRolesAsync(user);
        var userRole = userRoles.First() ?? "Volunteer";

        var jwtClaims = new JwtClaimsModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationName = user?.Foundation?.Name!,
        };

        var newRefreshToken = await _jwtTokenService.RotateRefreshTokenAsync(request.RefreshToken, user.Id, userRole);


        if (string.IsNullOrEmpty(newRefreshToken))
        {
            _logger.LogError("Failed to rotate refresh token for user {UserId}", request.UserId);
            return ApiResponse<RefreshTokenResponseModel>.Failure(400, "Failed to generate new refresh token");
        }

        var newAccessToken = _jwtTokenService.GenerateAccessTokenAsync(jwtClaims, userRole);

        var tokenExpirations = AuthenticationConstants.TokenExpirations.ContainsKey(userRole)
                    ? AuthenticationConstants.TokenExpirations[userRole]
                    : new TokenExpiration(AccessToken: 60, RefreshToken: 1440); // Default values

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.AccessToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.RefreshToken);


        _logger.LogInformation("Token refresh successful for user {UserId}. New tokens generated.", request.UserId);

        // Create response
        var refreshResponse = new RefreshTokenResponseModel
        {
            Success = true,
            Message = "Tokens refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            TokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };

        return ApiResponse<RefreshTokenResponseModel>.Success("Tokens refreshed successfully", refreshResponse);


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
    public async Task<ApiResponse<string>> ConfirmUser(ConfirmUserModel model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLower());
        if (user == null)
        {
            return ApiResponse<string>.Failure(404, "User not found.");
        }
        var confirmOtp = await _otp.ConfirmOtpAsync(user.Id, model.OtpCode, model.purpose);
        if (confirmOtp == true)
        {
            user.EmailConfirmed = true;
            user.IsActive = true;
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
        if (result.Succeeded)
        {
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            return ApiResponse<string>.Success("Password Successfully Changed", null);
        }
        else
        {
            return ApiResponse<string>.Failure(400, "Password change Fail");
        }
    }
    public async Task<ApiResponse<string>> ResendOTP(string email, OtpPurpose purpose)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
        if (user != null)
        {
            //verify if the otp is used
            var isUsed = await _otp.ConfirmOtpAsync(user.Id, user.OTP, purpose);
            if (isUsed == false)
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
            else if (isUsed == true)
            {
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
            else
            {
                return ApiResponse<string>.Failure(404, "Otp not found");
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