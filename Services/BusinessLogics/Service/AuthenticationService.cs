using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    public AuthenticationService(IUnitOfWork uow,
        IMapper mapper,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthenticationService> logger,
        IUserRepository userRepository)
    {
        _uow = uow;
        _mapper = mapper;
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _userRepository = userRepository;
        _logger = logger;

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

    public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailWithFoundationAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ApiResponse<LoginResponseModel>.Failure(401, "Invalid credentials");
        }

        if (!user.IsActive || user.Foundation?.IsActive != true)
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

        var roles = await _userManager.GetRolesAsync(user);
        var primaryRole = roles.FirstOrDefault() ?? "Volunteer";

        var jwtClaims = new JwtClaimsModel
        {
            Role = primaryRole,
            FirstName = user.FirstName,
            LastName = user.LastName,
            OrganizationName = user?.Foundation?.Name!,
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
            HasCompletedOnboarding = user.OnboardingProgress.HasCompletedOnboarding,
            LastCompletedPage = user.OnboardingProgress.LastCompletedPage,
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