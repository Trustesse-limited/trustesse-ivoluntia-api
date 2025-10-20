using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService;

public interface IAuthService
{
    Task<ApiResponse<string>> CreateVolunteer(VolunteerSignUpDto model);
    Task<ApiResponse<string>> ResetPasswordAsync(string email);
    Task<ApiResponse<string>> ChangePasswordAsync(ChangePasswordModel model);
    Task<ApiResponse<string>> ValidateOTP(string otp, string email);
    Task<ApiResponse<string>> ResendOTP(string email);
    Task<ApiResponse<string>> CreatePasswordAsync(ResetPasswordModel model);

}