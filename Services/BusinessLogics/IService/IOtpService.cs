using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, OtpPurpose purpose);
        Task<bool> ConfirmOtpAsync(string userId, string otpCode, OtpPurpose purpose);
    }
}
