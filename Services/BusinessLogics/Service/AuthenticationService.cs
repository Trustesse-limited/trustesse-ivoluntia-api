using CloudinaryDotNet;
using MapsterMapper;
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
    private readonly ITwoFactorAuthenticationService _twoFactorAuthenticationService;
    private readonly IOtpEmailSenderService _otpEmailSenderService;
    private readonly IUserMapperService _userMapperService;
    private readonly IConfiguration _configuration;
    private readonly byte[] _key;
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
        ICurrentUserService currentUserService,
        ITwoFactorAuthenticationService twoFactorAuthenticationService,
        IOtpEmailSenderService otpEmailSenderService,
        IUserMapperService userMapperService,
        IConfiguration configuration)
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
    }
    public async Task<GlobalRequestReponse<string>> CreateVolunteer(SignUpDto signUpDto)
    {
        var user = await _uow.userRepo.GetByExpressionAsync(x =>
        x.Email == signUpDto.Email);
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
        var user = await _uow.userRepo.GetByExpressionAsync(x =>
        x.Email == signUpDto.Email);
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

    public async Task<GlobalRequestReponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetUserByEmailWithFoundationAsync(request.Email, cancellationToken);

        if (user is null)
        {
            return ResponseHelper.BuildResponse<LoginResponseModel>("user not found , please sign up", StatusCodes.Status404NotFound, null, false);
        }

        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        string accountType = "";
        if(role == "Volunteer")
        {
            accountType = "Volunteer";
        }
        else
        {
            accountType = "Organization";
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
        _uow.userRepo.Update(user);
        await _uow.CompleteAsync();
        var hasSetUpPin = await _uow.transactionPinRepo.GetByExpressionAsync(x => x.UserId == user.Id) != null;
        var longinResponse = new LoginResponseModel
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            HasCompletedOnboarding = user.OnboardingProgress?.HasCompletedOnboarding ?? false,
            LastCompletedPage = user.OnboardingProgress?.LastCompletedPage ?? 0,
            HasSetUpPin = hasSetUpPin,
            Message = "Login successful",
            AccountType = accountType,  
        };
        return ResponseHelper.BuildResponse<LoginResponseModel>("login successful", StatusCodes.Status200OK, longinResponse, true);
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
    public async Task<GlobalRequestReponse<string>> ResetPasswordAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim().ToLower());
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
                //await _userManager.UpdateAsync(user).ConfigureAwait(false);
                return ResponseHelper.BuildResponse<string>("password change successfully", StatusCodes.Status200OK, null, true);
            }
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status404NotFound, null, false);
        }
        return ResponseHelper.BuildResponse<string>("user not found", StatusCodes.Status404NotFound, null, false);
    }
    
    public async Task<GlobalRequestReponse<string>> ForgetPasswordAsync(ForgotPasswordDto forgotPasswordDto)
    {
        var decrptedToken = AES.DecryptData(forgotPasswordDto.Token, _key);
        if(string.IsNullOrEmpty(decrptedToken))
            return ResponseHelper.BuildResponse<string>("invalid token", StatusCodes.Status400BadRequest, null, false);
        var payload = JsonSerializer.Deserialize<PasswordResetTokenPayload>(decrptedToken);
        var user = await _userManager.FindByEmailAsync(payload.Email); 
        if (user != null)
        {
            var passwordHash = _userManager.PasswordHasher.HashPassword(user, forgotPasswordDto.NewPassword);
            user.PasswordHash = passwordHash;
            var result = await _userManager.UpdateAsync(user);
            if(result.Succeeded)
                return ResponseHelper.BuildResponse<string>("password reset successfully", StatusCodes.Status200OK, null, true);
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

    public async Task<GlobalRequestReponse<VerifyTwoFactorAuthenticationResponseDto>> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto)
    {
        var user = await _userManager.FindByEmailAsync(verifyTwoFactorAuthenticationRequestDto.Email);
        var otp = await _uow.OtpRepo.GetByExpressionAsync(o => o.UserId == user.Id && o.OtpCode == verifyTwoFactorAuthenticationRequestDto.TwoFactorAuthCode && o.Purpose == OtpPurpose.TwoFactorAuthenticationLogin.ToString() && !o.IsUsed);
        if (otp == null)
        {
            await _userManager.AccessFailedAsync(user);
            return ResponseHelper.BuildResponse<VerifyTwoFactorAuthenticationResponseDto>("two factor authentication code is incorrect", StatusCodes.Status500InternalServerError, null, false);
        }
        if(user == null)
            return ResponseHelper.BuildResponse<VerifyTwoFactorAuthenticationResponseDto>("user not found", StatusCodes.Status404NotFound, null, false);
        if (await _userManager.IsLockedOutAsync(user))
        {
            return ResponseHelper.BuildResponse<VerifyTwoFactorAuthenticationResponseDto>("Account is locked for 1 hour due to multiple fail verification attempts", StatusCodes.Status400BadRequest, null, false);
        }
        
        if ((DateTime.UtcNow - otp.CreatedAt).TotalMinutes > 5)
            return ResponseHelper.BuildResponse<VerifyTwoFactorAuthenticationResponseDto>("two factor authentication code already expire", StatusCodes.Status400BadRequest, null, false);
        await _userManager.ResetAccessFailedCountAsync(user);
        var roles = await _userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault();
        string accountType = "";
        if (role == "Volunteer")
        {
            accountType = "Volunteer";
        }
        else
        {
            accountType = "Organization";
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
        user!.LastLogin = DateTime.UtcNow;
        _uow.userRepo.Update(user);
        await _uow.CompleteAsync();
        var response = VerifyTwoFactorAuthenticationResponseDtoBuilder.VerifyTwoFactorResponseBuilder(accessToken, refreshToken, accountType);
        return ResponseHelper.BuildResponse<VerifyTwoFactorAuthenticationResponseDto>("something went wrong", StatusCodes.Status400BadRequest, response, false);
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