using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthenticationService
{
    Task<ApiResponse<string>> CreateVolunteer(VolunteerSignUpDto model);
    Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
    Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken);
    Task<ApiResponse<string>> ResetPasswordAsync(string email);
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordModel model);
    Task<ApiResponse<string>> ConfirmUser(ConfirmUserModel model);
    Task<ApiResponse<string>> ResendOTP(string email, OtpPurpose purpose);
    Task<ApiResponse<string>> CreatePasswordAsync(ResetPasswordModel model);
}