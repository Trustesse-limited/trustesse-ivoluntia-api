using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json;
using System.Web;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.Cryptography;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.Abstractions;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Net.WebRequestMethods;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUserRepository _userRepository;
    private readonly IOtpService _otp;
    private readonly INotificationService _notify;
    private readonly IEmailService _email;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
    private readonly IOtpEmailSenderService _otpEmailSenderService;
    private readonly IUserMapperService _userMapperService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _http;
    private readonly byte[] _key;
    public AuthService(IUnitOfWork uow,
        IMapper mapper,
        UserManager<User> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IOtpService otp,
        INotificationService notify,
        IEmailService email,
        IUserRepository userRepository,
        IFileUploadService fileUploadService,
        ICurrentUserService currentUserService,
        ITwoFactorAuthenticationService twoFactorAuthenticationService,
        IOtpEmailSenderService otpEmailSenderService,
        IUserMapperService userMapperService,
        IConfiguration configuration,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor http
        )
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
        _twoFactorAuthenticationService = twoFactorAuthenticationService;
        _otpEmailSenderService = otpEmailSenderService;
        _userMapperService = userMapperService;
        _configuration = configuration;
        var key = configuration["PasswordResetToken:Key"];
        _key = Convert.FromBase64String(key);
        _dataProtectionProvider = dataProtectionProvider;
        _http = http;
    }

    public async Task<GlobalRequestReponse<string>> CreateVolunteer(SignUpDto signUpDto)
    {
        var user = await _userManager.FindByEmailAsync(signUpDto.Email);
        if (user != null)
            return ResponseHelper.BuildResponse<string>($"user already exist. please sign in", StatusCodes.Status400BadRequest, null, false);
        var volunteer = await _userMapperService.UserMapper(signUpDto);
        var result = await _userManager.CreateAsync(volunteer, signUpDto.Password.Trim());
        await _userManager.AddToRoleAsync(volunteer, UserRolesEnum.Volunteer.ToString());
        if (result.Succeeded)
        {
            var response = await _otpEmailSenderService.OtpSender(volunteer.Email, volunteer.OTP, NotificationTypeEnum.EmailConfirmationOtp.ToString());
            return response;
        }
        return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
    }
   
    public async Task<GlobalRequestReponse<string>> CreateOrganization(SignUpDto signUpDto)
    {
        var user = await _userManager.FindByEmailAsync(signUpDto.Email);
        if (user != null)
            return ResponseHelper.BuildResponse<string>($"user already exist, please log in.", StatusCodes.Status400BadRequest, null, false);
        var foundationAdmin = await _userMapperService.UserMapper(signUpDto);
        var result = await _userManager.CreateAsync(foundationAdmin, signUpDto.Password.Trim());
        await _userManager.AddToRoleAsync(foundationAdmin, UserRolesEnum.FoundationAdmin.ToString());
        if (result.Succeeded)
        {
            // emailService 
            var response = await _otpEmailSenderService.OtpSender(foundationAdmin.Email, foundationAdmin.OTP, NotificationTypeEnum.EmailConfirmationOtp.ToString());
            return response; 
        }
        return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
    }
    public async Task<GlobalRequestReponse<LoginResponseModel>> LoginAsync(LoginRequestModel request)
    {
        var user = await _uow.userRepo.GetByExpressionIncludeAsync(u => u.Email == request.Email, u => u.OnboardingProgress, u => u.UserInterestLinks, u => u.UserSkillLinks, u => u.Location, u => u.Location.Country, u => u.Location.State, u => u.Foundation, u => u.Foundation.Category,u =>  u.Foundation.Location.State, u => u.Foundation.Location.Country, u => u.Foundation.Causes);
        var interest = new List<UserInterestLink>(); 
        var skill = new List<UserSkillLink>();  
        if (user is null)
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("user not found , please sign up", StatusCodes.Status404NotFound, null, false);
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return ResponseHelper.BuildResponse<LoginResponseModel>("invalid credentials", StatusCodes.Status400BadRequest, null, false);
        }
        if (user.OnboardingProgress != null && user.OnboardingProgress.LastCompletedPage > 0 && user.OnboardingProgress.LastCompletedPage < 5 && user.OnboardingProgress.HasCompletedOnboarding == false)
        {
            interest = await _uow.userInterestLinkRepo.GetListByExpressionAsync(i => i.UserId == user.Id, i => i.Interest);
            skill = await _uow.userSkillLinkRepo.GetListByExpressionAsync(i => i.UserId == user.Id, i => i.Skill);
        }
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        string accountType = "";
        if(role == "Volunteer")
        {
            accountType = "Volunteer";
        }
        else if(role == "FoundationAdmin")
        {
            accountType = "Foundation";
        }
        else
        {
            accountType = "SuperAdmin";
        }
        if (!user.IsActive)
        {
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.Signup.ToString(), false, NotificationChannelEnum.Email.ToString());
            user.OTP = otp;
            var response = await _otpEmailSenderService.OtpSender(user.Email, user.OTP, NotificationTypeEnum.EmailConfirmationOtp.ToString());
            if(response.isSuccessfull)
                return ResponseHelper.BuildResponse<LoginResponseModel>("your account has not been confirm , please confirm your email with the OTP sent to your email", StatusCodes.Status400BadRequest, null, false);
            return ResponseHelper.BuildResponse<LoginResponseModel>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("account lock due to failed attempt", StatusCodes.Status403Forbidden, null, false);
        } 
        if (!user.IsActive)
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("account not active, please confirm your email", StatusCodes.Status400BadRequest, null, false); 
        }
        if (await _userManager.IsLockedOutAsync(user))
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("account lock due to failed attempt",StatusCodes.Status403Forbidden, null, false);
        }

        if (!await _userManager.CheckPasswordAsync(user, request.Password))
        {
            await _userManager.AccessFailedAsync(user);
            return ResponseHelper.BuildResponse<LoginResponseModel>("wrong password", StatusCodes.Status400BadRequest, null,false);
        }
        await _userManager.ResetAccessFailedCountAsync(user);
        if(user.TwoFactorEnabled)
        {
            var response = await _twoFactorAuthenticationService.TwoFactorAuthenticationByEmail(user.Email);
            if(response.ResponseCode == StatusCodes.Status200OK)
                return ResponseHelper.BuildResponse<LoginResponseModel>(response.Message, response.ResponseCode, null, true);
            return ResponseHelper.BuildResponse<LoginResponseModel>(response.Message, response.ResponseCode, null, false);
        }
        user.LastLogin = DateTime.UtcNow;
        user.DateUpdated = DateTime.UtcNow;
        var jwtClaims = _mapper.Map<JwtClaimsModel>(user);
        jwtClaims.UserId = user.Id; 
        jwtClaims.Role = role;
        if(user.Foundation != null)
        {
            jwtClaims.OrganizationName = user.Foundation.Name;
            jwtClaims.FoundationId = user.Foundation.Id;
        }
        var accessToken = _jwtTokenService.GenerateAccessTokenAsync(jwtClaims, role);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(
            user?.Id!, role);
        user!.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user); 
        var hasSetUpPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == user.Id) != null;
        //volunteer data
        if (user.OnboardingProgress != null && user.FoundationId == null && user.OnboardingProgress.LastCompletedPage > 0 && user.OnboardingProgress.LastCompletedPage < 5 && user.OnboardingProgress.HasCompletedOnboarding == false)
        {
            var loginResponse = _mapper.Map<LoginResponseModel>(user);
            loginResponse.AccountType = accountType;
            loginResponse.AccessToken = accessToken;
            loginResponse.AccessTokenExpireMinutes = DateTime.Now.AddMinutes(30);
            loginResponse.RefreshToken = refreshToken;
            loginResponse.RefreshTokenExpireDays = DateTime.Now.AddDays(30);    
            loginResponse.HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? false;
            loginResponse.LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0;
            loginResponse.NumberOfPageRemaining = 5 - user.OnboardingProgress?.LastCompletedPage ?? 5;
            loginResponse.HasSetUpPin = hasSetUpPin;
            loginResponse.Message = "Login successful";
            loginResponse.UserProfile = _mapper.Map<UserProfileSummary>(user);
            loginResponse.UserProfile.Address = user.Location?.Address ?? null;
            loginResponse.UserProfile.City = user.Location?.City ?? null;
            loginResponse.UserProfile.ZipCode = user.Location?.Zipcode ?? null;
            loginResponse.UserProfile.CountryName = user.Location?.Country.CountryName ?? null;
            loginResponse.UserProfile.StateName = user.Location?.State.StateName ?? null;
            loginResponse.UserProfile.InterestNames = interest?.Select(i => i.Interest.Name).ToList() ?? null;
            loginResponse.UserProfile.SkillNames = skill?.Select(s => s.Skill.Name).ToList() ?? null ;
            return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, loginResponse, true);
        }
        // foundation data
        else if(user.OnboardingProgress != null && user.FoundationId != null && user.OnboardingProgress.LastCompletedPage > 0 && user.OnboardingProgress.LastCompletedPage < 5 && user.OnboardingProgress.HasCompletedOnboarding == false)
        {
            var loginResponse = _mapper.Map<LoginResponseModel>(user);
            loginResponse.AccountType = accountType;
            loginResponse.AccessToken = accessToken;
            loginResponse.AccessTokenExpireMinutes = DateTime.Now.AddMinutes(30);
            loginResponse.RefreshToken = refreshToken;
            loginResponse.RefreshTokenExpireDays = DateTime.Now.AddDays(30);
            loginResponse.HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? false;
            loginResponse.LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0;
            loginResponse.NumberOfPageRemaining = 5 - user.OnboardingProgress?.LastCompletedPage ?? 5;
            loginResponse.HasSetUpPin = hasSetUpPin;
            loginResponse.Message = "Login successful";
            loginResponse.UserProfile = _mapper.Map<UserProfileSummary>(user);
            loginResponse.UserProfile.Name = user.Foundation?.Name ?? null;
            loginResponse.UserProfile.Category = user.Foundation?.Category?.Name ?? null;
            loginResponse.UserProfile.Website = user.Foundation?.Website ?? null;
            loginResponse.UserProfile.Mission = user.Foundation?.Mission ?? null;
            loginResponse.UserProfile.Address = user.Foundation?.Location?.Address ?? null;
            loginResponse.UserProfile.City = user.Foundation?.Location?.City ?? null;
            loginResponse.UserProfile.ZipCode = user.Foundation?.Location?.Zipcode?? null;
            loginResponse.UserProfile.CountryName = user.Foundation?.Location?.Country?.CountryName ?? null;
            loginResponse.UserProfile.StateName = user.Foundation?.Location?.State?.StateName ?? null;
            loginResponse.UserProfile.CauseNames = user.Foundation?.Causes?.Select(fc => fc.Name).ToList() ?? null;
            loginResponse.UserProfile.FoundationLogoUrl = user.Foundation?.Logo ?? null  ;
            return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, loginResponse, true);
        }
        //completed onboarding or not started onboarding
        else
        {
            var loginResponse = _mapper.Map<LoginResponseModel>(user);
            loginResponse.AccessToken = accessToken;
            loginResponse.AccessTokenExpireMinutes = DateTime.Now.AddMinutes(30);
            loginResponse.RefreshToken = refreshToken;
            loginResponse.RefreshTokenExpireDays = DateTime.Now.AddDays(30);
            loginResponse.HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? false;
            loginResponse.LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0;
            loginResponse.NumberOfPageRemaining = 5 - user.OnboardingProgress?.LastCompletedPage ?? 5;
            loginResponse.HasSetUpPin = hasSetUpPin;
            loginResponse.Message = "Login successful";
            loginResponse.AccountType = accountType;
            loginResponse.UserProfile = _mapper.Map<UserProfileSummary>(user);
            return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, loginResponse, true);
        } 
    }

    public async Task<GlobalRequestReponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request)
    {
        var principal = _jwtTokenService.GetPrincipalFromExpiredToken(request.AccessToken);
        var email = principal.Identity.Name;
        var auth = principal.Identity.IsAuthenticated;
        var validation = await _jwtTokenService.ValidateRefreshTokenAsync(request.RefreshToken, email);
        if (!validation.IsValid)
        {
            _logger.LogWarning("Invalid refresh token used. Status: {Status}, Error: {Error}",
                validation.Status, validation.ValidationError);
            return ResponseHelper.BuildResponse<RefreshTokenResponseModel>("invalid refresh token", StatusCodes.Status400BadRequest,null, false);
        }
        if (validation.User is null)
        {
            return ResponseHelper.BuildResponse<RefreshTokenResponseModel>("user not found", StatusCodes.Status404NotFound, null, false);
        }
        var userRoles = await _userManager.GetRolesAsync(validation.User);
        var userRole = userRoles.FirstOrDefault();
        var jwtClaims = _mapper.Map<JwtClaimsModel>(validation.User);
        jwtClaims.UserId = validation.User.Id;
        jwtClaims.Role = userRole;
        if (validation.User.Foundation != null)
        {
            jwtClaims.OrganizationName = validation.User.Foundation.Name;
            jwtClaims.FoundationId = validation.User.Foundation.Id;
        }
        var newRefreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(validation.User.Id, userRole);
        if (string.IsNullOrEmpty(newRefreshToken))
        {
            _logger.LogError($"Failed to rotate refresh token for user {validation.User.Id}");
            return ResponseHelper.BuildResponse<RefreshTokenResponseModel>("Failed to generate new refresh token", StatusCodes.Status400BadRequest, null, false);
        }
        var newAccessToken = _jwtTokenService.GenerateAccessTokenAsync(jwtClaims, userRole);
        var tokenExpirations = AuthenticationConstants.TokenExpirations.ContainsKey(userRole)
                    ? AuthenticationConstants.TokenExpirations[userRole]
                    : new TokenExpiration(AccessToken: 15, RefreshToken: 30); // Default values
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.AccessToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(tokenExpirations.RefreshToken);
        _logger.LogInformation($"refresh token generated for user {validation.User.Id}.");
        var refreshResponse = new RefreshTokenResponseModel
        {
            Success = true,
            Message = "Tokens refreshed successfully",
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };
        return ResponseHelper.BuildResponse<RefreshTokenResponseModel>("token refresh successfully", StatusCodes.Status200OK, refreshResponse, true);
    }
    public async Task<GlobalRequestReponse<string>> ResetPasswordAsync()
    {
        var user = await _userManager.FindByEmailAsync(_currentUserService.GetUserEmail().Trim().ToLower());
        if (user != null)
        {
            //Generate OTP
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.PasswordReset.ToString(),true, NotificationChannelEnum.Email.ToString());
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
            }
            else
            {
                var response = await _otpEmailSenderService.OtpSender(user.Email, user.OTP, NotificationTypeEnum.ResetPasswordOtp.ToString());
                return response;    
            }
        }
        return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
    }
   
    public async Task<GlobalRequestReponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto)
    {
        var decrptedToken = AES.DecryptData(changePasswordDto.Token, _key);
        if (string.IsNullOrEmpty(decrptedToken))
            return ResponseHelper.BuildResponse<string>("invalid token", StatusCodes.Status400BadRequest, null, false);
        var payload = JsonSerializer.Deserialize<PasswordResetTokenPayload>(decrptedToken);
        var user = await _userManager.FindByEmailAsync(payload.Email.Trim().ToLower());
        if (user != null)
        {
            var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.OldPassword.Trim(), changePasswordDto.NewPassword.Trim());
            if (result.Succeeded)
            {
                await _userManager.UpdateAsync(user);
                var refresh = await _uow.refreshTokenRepo.GetByExpressionAsync(r => r.UserId == user.Id);
                if (refresh != null)
                {
                    refresh.ExpiresAt = DateTime.UtcNow;
                    _uow.refreshTokenRepo.Update(refresh);
                    await _uow.CompleteAsync();
                }
                return ResponseHelper.BuildResponse<string>("password change successfully", StatusCodes.Status200OK, null, true);
            }
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status404NotFound, null, false);
        }
        return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
    }
    public async Task<GlobalRequestReponse<string>> ResetForgotPassword(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
        if (user != null)
        {
            //Generate OTP
            var otp = await _otp.GenerateOtpAsync(user.Id, OtpPurpose.ForgotPassWord.ToString(), true, NotificationChannelEnum.Email.ToString());
            user.OTP = otp;
            user.OtpSubmittedTime = Convert.ToDateTime(DateTime.Now.ToShortTimeString());
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
            }
            else
            {
                var response = await _otpEmailSenderService.OtpSender(user.Email, user.OTP, NotificationTypeEnum.ForgotPassWord.ToString());
                return response;
            }
        }
        return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
    }
    public async Task<GlobalRequestReponse<string>> ForgetPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var decrptedToken = AES.DecryptData(forgotPasswordDto.Token, _key);
        if (string.IsNullOrEmpty(decrptedToken))
            return ResponseHelper.BuildResponse<string>("invalid token", StatusCodes.Status400BadRequest, null, false);
        var payload = JsonSerializer.Deserialize<PasswordResetTokenPayload>(decrptedToken);
        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user != null)
        {
            var passwordHash = _userManager.PasswordHasher.HashPassword(user, forgotPasswordDto.NewPassword);
            user.PasswordHash = passwordHash;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                var refresh = await _uow.refreshTokenRepo.GetByExpressionAsync(r => r.UserId == user.Id);
                if (refresh != null)
                {
                    refresh.ExpiresAt = DateTime.UtcNow;
                    _uow.refreshTokenRepo.Update(refresh);
                    await _uow.CompleteAsync();
                }
                return ResponseHelper.BuildResponse<string>("password reset successfully", StatusCodes.Status200OK, null, true);
            }  
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
        return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false); ;
    }
    
  
    public async Task<GlobalRequestReponse<string>> TwoFactorAuthenticationSetUp()
    {
        var email = _currentUserService.GetUserEmail(); 
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) 
            return ResponseHelper.BuildResponse("user not found", StatusCodes.Status404NotFound, "not found", false);
        if(!user.EmailConfirmed)
            return ResponseHelper.BuildResponse("user email not confirmed", StatusCodes.Status400BadRequest, "email not confirmed", false);
        user.TwoFactorEnabled = true;
        var response = await _userManager.UpdateAsync(user);
        if(response.Succeeded)
            return ResponseHelper.BuildResponse("two factor authenticaton is enable", StatusCodes.Status200OK, "success", true);
        return ResponseHelper.BuildResponse("something went wrong", StatusCodes.Status400BadRequest, "not successful", false);
    }
    public async Task<GlobalRequestReponse<LoginResponseModel>> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto)
    {
        var user = await _userManager.FindByEmailAsync(verifyTwoFactorAuthenticationRequestDto.Email);
        var otp = await _uow.OtpRepo.GetByExpressionAsync(o => o.UserId == user.Id && o.OtpCode == verifyTwoFactorAuthenticationRequestDto.TwoFactorAuthCode && o.Purpose == OtpPurpose.TwoFactorAuthenticationLogin.ToString() && !o.IsUsed);
        if(user == null)
            return ResponseHelper.BuildResponse<LoginResponseModel>("user not found", StatusCodes.Status404NotFound, null, false);
        if (await _userManager.IsLockedOutAsync(user))
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("Account is locked for 1 hour due to multiple fail verification attempts", StatusCodes.Status400BadRequest, null, false);
        }
        if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
            return ResponseHelper.BuildResponse<LoginResponseModel>("two factor authentication code already expire", StatusCodes.Status400BadRequest, null, false);
        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        string accountType = "";
        if (role == "Volunteer")
        {
            accountType = "Volunteer";
        }
        else if (role == "FoundationAdmin")
        {
            accountType = "Foundation";
        }
        else
        {
            accountType = "SuperAdmin";
        }
        user.LastLogin = DateTime.UtcNow;
        user.DateUpdated = DateTime.UtcNow; 
        var jwtClaims = _mapper.Map<JwtClaimsModel>(user);
        jwtClaims.UserId = user.Id; 
        jwtClaims.Role = role;
        if(user.Foundation != null)
        {
            jwtClaims.OrganizationName = user.Foundation.Name;
            jwtClaims.FoundationId = user.FoundationId;
        }
        var accessToken = _jwtTokenService.GenerateAccessTokenAsync(jwtClaims, role);
        var refreshToken = await _jwtTokenService.GenerateRefreshTokenAsync(
            user?.Id!, role);
        var tokenExpirations = AuthenticationConstants.TokenExpirations.ContainsKey(role)
                    ? AuthenticationConstants.TokenExpirations[role]
                    : new TokenExpiration(AccessToken: 15, RefreshToken: 30); // Default values
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(tokenExpirations.AccessToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(tokenExpirations.RefreshToken);
        user!.LastLogin = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
        var hasSetUpPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == user.Id) != null;
        if (role == "SuperAdmin")
        {
            var loginResponse = _mapper.Map<LoginResponseModel>(user);
            loginResponse.AccountType = accountType;
            loginResponse.AccessToken = accessToken;
            loginResponse.RefreshToken = refreshToken;
            loginResponse.AccessTokenExpireMinutes = accessTokenExpiresAt;
            loginResponse.RefreshTokenExpireDays = refreshTokenExpiresAt;
            loginResponse.HasCompletedOnboarding = true;
            loginResponse.LastCompletedPage = 5;
            loginResponse.NumberOfPageRemaining = 0;
            loginResponse.HasSetUpPin = hasSetUpPin;
            loginResponse.Message = "Login successful";
            loginResponse.UserProfile = _mapper.Map<UserProfileSummary>(user);
            return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, loginResponse, true);
        } 
        else
        {
            var loginResponse = _mapper.Map<LoginResponseModel>(user);
            loginResponse.AccessToken = accessToken;
            loginResponse.RefreshToken = refreshToken;
            loginResponse.AccessTokenExpireMinutes = accessTokenExpiresAt;
            loginResponse.RefreshTokenExpireDays = refreshTokenExpiresAt;
            loginResponse.HasCompletedOnboarding = user.OnboardingProgress.HasCompletedOnboarding;
            loginResponse.LastCompletedPage = user.OnboardingProgress.LastCompletedPage;
            loginResponse.NumberOfPageRemaining = 5 - user.OnboardingProgress.LastCompletedPage;
            loginResponse.HasSetUpPin = hasSetUpPin;
            loginResponse.Message = "Login successful";
            loginResponse.AccountType = accountType;
            loginResponse.UserProfile = _mapper.Map<UserProfileSummary>(user);
            return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, loginResponse, true);
        }
    }
    public async Task<GlobalRequestReponse<string>>Logout()
    {
        var user = await _userManager.FindByIdAsync(_currentUserService.GetUserId());
        if (user == null)
        {
            return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
        }
        var userRefreshToken = await _uow.refreshTokenRepo.GetByExpressionIncludeAsync(r => r.UserId == user.Id, r => r.User);
        if (userRefreshToken == null)
        {
            return ResponseHelper.BuildResponse<string>("no refresh token found for user", StatusCodes.Status404NotFound, null, false);
        }
        userRefreshToken.ExpiresAt = DateTime.Now;
        await _uow.refreshTokenRepo.UpdateAsync(userRefreshToken);
        await _uow.CompleteAsync();
        return ResponseHelper.BuildResponse<string>("logout successfully", StatusCodes.Status200OK, "logout", true);
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