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
    public async Task<GlobalRequestReponse<string>> CreateVolunteer(VolunteerSignUpDto model)
    {
        if (_currentUserService.GetUserEmail() == null)
        {
            var VolunteerExists = await _uow.userRepo.GetByExpressionAsync(x =>
                x.Email == model.AuthInfo.Email);
            if (VolunteerExists != null)
                return ResponseHelper.BuildResponse($"Volunteer with Email -> {model.AuthInfo.Email}  already exist.", StatusCodes.Status400BadRequest, "user already exist", false);
            var volunteer = _mapper.Map<User>(model);
            volunteer.UserName = model.AuthInfo.Email;
            volunteer.Email = model.AuthInfo.Email.Trim();
            volunteer.DateCreated = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
            volunteer.IsActive = false;
            volunteer.HasAgreedToTermsAndCondition = model.AuthInfo.HasAgreedToTermsAndCondition;
            var otp = GenerateOTP();
            volunteer.OTP = otp;
            volunteer.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.CreateAsync(volunteer, model.AuthInfo.Password.Trim());
            await _userManager.AddToRoleAsync(volunteer, UserRolesEnum.Volunteer.ToString());
            if (result.Succeeded)
            {
                    // emailService 
                    var dictionary = new Dictionary<string, string>()
                {
                {"firstname",volunteer.Email},
                { "otp", volunteer.OTP}
                };
                    var notificationTemplate = await _notify.ComposeNotificationAsync(NotificationTypeEnum.OtpRequest.ToString(), NotificationChannelEnum.Email.ToString(), dictionary);
                    if (notificationTemplate != null)
                    {
                        var message = new EmailModel
                        {
                            Receivers = new List<string> { volunteer.Email },
                            Subject = "OTP",
                            Message = HttpUtility.HtmlDecode(notificationTemplate.Data)
                        };
                        var emailResponse = await _email.SendEmailASync(message);
                    }
                    var otpDto = _mapper.Map<OtpDto>(volunteer);
                    otpDto.IsUsed = false;
                    otpDto.Purpose = OtpPurpose.Signup.ToString();
                    otpDto.OtpCode = volunteer.OTP;
                    otpDto.Channel = NotificationChannelEnum.Email.ToString();
                    otpDto.UserId = volunteer.Id;
                    var mapOtp = _mapper.Map<Otp>(otpDto);
                    await _uow.OtpRepo.AddAsync(mapOtp);
                    await AddOnBoardingProgress(volunteer.Id, (int)OnBoardingPages.AuthInfoPage, false, 6);
                return ResponseHelper.BuildResponse("user register and otp sent", StatusCodes.Status200OK, "created successfully", true);
            }
            return ResponseHelper.BuildResponse("unable to register user", StatusCodes.Status400BadRequest, "not successful", false);
        }
        if (model.BioData != null)
        {
            await UpdateBioData(model.BioData);
        }
        if (model.LocationDto != null)
        {
            await UpdateLocation(model.LocationDto);
        }
        if (model.Interest != null)
        {
            await UpdateUserInterest(model.Interest);
        }

        if (model.Skill != null)
        {
            await UpdateUserSkill(model.Skill);
        }

        if (model.ProfileAndBioData != null)
        { 
            var imageUrl = await _fileUploadService.UploadFilesAsync(model.ProfileAndBioData.ProfileImage);
            var user = await _uow.userRepo.GetByExpressionAsync(u => u.Email == _currentUserService.GetUserEmail());
            user.UserImage = imageUrl.Data[0];
            await _userManager.UpdateAsync(user);
            await AddOnBoardingProgress(user.Id, (int)OnBoardingPages.ProfileImageAndBio, false, 6);
            return ResponseHelper.BuildResponse("onboarding completed", StatusCodes.Status200OK, "created successfully", true);
        }
        return ResponseHelper.BuildResponse("unexpected result", StatusCodes.Status400BadRequest, "not successful", false);
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
            await UpdateOnBoardingProgress(volunteer.Id, 2, false);
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
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), 3, false);
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
        var response = await UpdateOnBoardingProgress(volunteer.Id, 4, false);
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
        var response = await UpdateOnBoardingProgress(volunteer.Id, 5, false);
        if(response.StatusCode == StatusCodes.Status200OK)
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

    public async Task<GlobalRequestReponse<string>> CreateOrganization(CreateFoundationRequestDto createFoundationRequestDto)
    {
        if (_currentUserService.GetUserEmail() == null)
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
                var response = await AddOnBoardingProgress(mapFoundationAdmin.Id, (int)OrganizationOnboardingEnum.AuthInfoPage, false,6);
                if(response.StatusCode == StatusCodes.Status200OK)
                    return ResponseHelper.BuildResponse("foundation admin created", StatusCodes.Status200OK, "created successfully", true);
                return ResponseHelper.BuildResponse("unsuccessful", StatusCodes.Status400BadRequest, "not successful", false);
            }
        }
        if (createFoundationRequestDto.foundationBioData != null)
        {
            var mapOrganization = _mapper.Map<Foundation>(createFoundationRequestDto.foundationBioData);
            mapOrganization.Email = _currentUserService.GetUserEmail();
            var category = await _uow.CategoryRepository.GetByExpressionAsync(c => c.Name == createFoundationRequestDto.foundationBioData.FoundationCategory);
            mapOrganization.CategoryId = category.Id;
            var foundationAdmin = await _userManager.FindByEmailAsync(_currentUserService.GetUserEmail());
            foundationAdmin.FoundationId = mapOrganization.Id;
            mapOrganization.Status = OrganizationStatusUpdateEnums.Pending.ToString();
            await _uow.OrganizationRepository.AddAsync(mapOrganization);
            await _uow.CompleteAsync();
            var response = await _userManager.UpdateAsync(foundationAdmin);
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.BioDataPage, false);
        }
        if (createFoundationRequestDto.FoundationLocationDto != null)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            createFoundationRequestDto.FoundationLocationDto.UserId = _currentUserService.GetUserId();
            var locationId = await AddLocation(createFoundationRequestDto.FoundationLocationDto, foundation.Id);
            foundation.LocationId = locationId;
            _uow.OrganizationRepository.Update(foundation);
            var response = await _uow.CompleteAsync();
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Location, false);
        }
        if (createFoundationRequestDto.CauseDto != null)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            var response = await AddFoundationCause(createFoundationRequestDto.CauseDto.Names, foundation.Id, _currentUserService.GetUserEmail());
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Cause, false);
        }

        if (createFoundationRequestDto.ProfileLogo != null)
        {
            var imageUrl = await _fileUploadService.UploadFilesAsync(createFoundationRequestDto.ProfileLogo.Logo);
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            foundation.Logo = imageUrl.Data[0];
            _uow.OrganizationRepository.Update(foundation);
            var response = await _uow.CompleteAsync();
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Profile, false);
        }

        if(createFoundationRequestDto.Disclaimer!= null)
        {
            var foundation = await _uow.OrganizationRepository.GetByExpressionAsync(f => f.Email == _currentUserService.GetUserEmail());
            foundation.HasAgreedToDisclaimer = createFoundationRequestDto.Disclaimer.HasAgreedToDisclaimer;
            foundation.HasAgreedToDisclaimer = createFoundationRequestDto.Disclaimer.HasAgreedToDisclaimer;
            foundation.IsActive = true;
            _uow.OrganizationRepository.Update(foundation);
            await _uow.CompleteAsync();
            await UpdateOnBoardingProgress(_currentUserService.GetUserId(), (int)OrganizationOnboardingEnum.Disclaimer, true);
            return ResponseHelper.BuildResponse("success", StatusCodes.Status200OK, "onboarding completed", true);
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

        if (!user.IsActive)
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
        _uow.userRepo.Update(user);
        await _uow.CompleteAsync();

        var hasSetUpPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == user.Id) != null;

        var longinResponse = new LoginResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? true,
            LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0,
            HasSetUpPin = hasSetUpPin,
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
    public async Task<ApiResponse<string>> ConfirmUser(string otpCode, string otpPurpose)
    {
        var otp = await _otp.ConfirmOtpAsync(otpCode, otpPurpose);
        var user = await _userManager.FindByIdAsync(otp.Data.UserId);
        if (user != null)
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
            var isUsed = await _otp.ConfirmOtpAsync(user.OTP, purpose.ToString());
            if (isUsed.StatusCode == StatusCodes.Status200OK)
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