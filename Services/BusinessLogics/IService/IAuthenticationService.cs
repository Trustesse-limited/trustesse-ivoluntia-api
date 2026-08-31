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
    Task<GlobalRequestReponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
    Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken);
    Task<GlobalRequestReponse<string>> ResetPasswordAsync(string email);
    Task<GlobalRequestReponse<string>> ChangePasswordAsync(ChangePasswordDto changePasswordDto);
    Task<GlobalRequestReponse<string>> ForgetPasswordAsync(ForgotPasswordDto forgotPasswordDto);
    Task<GlobalRequestReponse<string>> TwoFactorAuthenticationSetUp();
    Task<GlobalRequestReponse<VerifyTwoFactorAuthenticationResponseDto>> VerifyTwoFactorAuthentication(VerifyTwoFactorAuthenticationRequestDto verifyTwoFactorAuthenticationRequestDto); 
}