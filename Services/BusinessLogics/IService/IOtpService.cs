using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, string purpose, bool includeAlphabet, string channel);
        Task<ApiResponse<Otp>> ConfirmOtpAsync(string otpCode, string otpPurpose);
        Task<GlobalRequestReponse<string>> ConfirmEmail(string otpCode, string otpPurpose);
        Task<GlobalRequestReponse<string>> VerifyResetPasswordOtp(string otpCode);
<<<<<<< Updated upstream
        Task<GlobalRequestReponse<string>> VerifyForgotPasswordOtp(string otpCode);
=======
>>>>>>> Stashed changes
        Task<GlobalRequestReponse<string>> ResendOTP(string email, string purpose, bool includeAlphabet, string channel, string notificationType);
    }
}
