using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.DTOs.Foundation;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthenticationService
{
    Task<GlobalRequestReponse<string>> CreateVolunteer(SignUpDto signUpDto);
    Task<GlobalRequestReponse<string>> CreateOrganization(SignUpDto signUpDto);
<<<<<<< Updated upstream
    Task<GlobalRequestReponse<LoginResponseModel>> LoginAsync(LoginRequestModel request);
    Task<GlobalRequestReponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request);
    Task<GlobalRequestReponse<string>> ResetPasswordAsync();
    Task<GlobalRequestReponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<GlobalRequestReponse<string>> ResetForgotPassword(string email);
    Task<GlobalRequestReponse<string>> ForgetPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<GlobalRequestReponse<string>> TwoFactorAuthenticationSetUp();
    Task<GlobalRequestReponse<LoginResponseModel>> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto);
    Task<GlobalRequestReponse<string>> Logout();
=======
    Task<GlobalRequestReponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
    Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken);
    Task<GlobalRequestReponse<string>> ResetPasswordAsync(string email);
    Task<GlobalRequestReponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<GlobalRequestReponse<string>> ForgetPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<GlobalRequestReponse<string>> TwoFactorAuthenticationSetUp();
    Task<GlobalRequestReponse<VerifyTwoFactorAuthenticationResponseDto>> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto); 
>>>>>>> Stashed changes
}