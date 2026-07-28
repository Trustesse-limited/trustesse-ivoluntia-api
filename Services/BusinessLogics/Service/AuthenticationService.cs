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
               {"Name",volunteer.Email},
               {"Otp", volunteer.OTP}
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
            await _uow.CompleteAsync();
            return ResponseHelper.BuildResponse("account created and otp sent", StatusCodes.Status200OK, "created successfully", true);
        }
        return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, "not successful", false);
    }
   
    public async Task<GlobalRequestReponse<string>> CreateOrganization(CreateFoundationRequestDto createFoundationRequestDto)
    {
        var mapFoundationAdmin = _mapper.Map<User>(createFoundationRequestDto.FoundationAdminInfo);
        var foundationAdminCheck = await _uow.userRepo.GetByExpressionAsync(x =>
        x.Email == createFoundationRequestDto.FoundationAdminInfo.Email);
        if (foundationAdminCheck != null)
            return ResponseHelper.BuildResponse("user already exist", StatusCodes.Status400BadRequest, "user exist", false);
        mapFoundationAdmin.UserName = createFoundationRequestDto.FoundationAdminInfo.Email;
        mapFoundationAdmin.PasswordHash = createFoundationRequestDto.FoundationAdminInfo.Password;         mapFoundationAdmin.Email = createFoundationRequestDto.FoundationAdminInfo.Email.Trim();
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
              {"Name", mapFoundationAdmin.Email},
              {"Otp", mapFoundationAdmin.OTP}
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
            await _uow.CompleteAsync(); 
            return ResponseHelper.BuildResponse("account created", StatusCodes.Status200OK, "otp sent", true);    
        }
        return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, "not successful", false);
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