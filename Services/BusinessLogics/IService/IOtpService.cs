using System.Threading.Tasks;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IOtpService
    {
        Task<string> GenerateOtpAsync(string userId, PurposeEnum purpose);
        Task<bool> ConfirmOtpAsync(string userId, string otpCode, PurposeEnum purpose);
    }
}
