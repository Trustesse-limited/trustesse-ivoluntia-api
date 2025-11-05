using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Auth;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthenticationService
{
    Task<ApiResponse<string>> CreateVolunteer(VolunteerSignUpDto model);
    Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel request, CancellationToken cancellationToken);
    Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(RefreshTokenRequestModel request, CancellationToken cancellationToken);
}