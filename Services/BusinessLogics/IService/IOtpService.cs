using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, OtpPurpose purpose);
        Task<ApiResponse<Otp>> ConfirmOtpAsync(string otpCode, string otpPurpose);
    }
}
