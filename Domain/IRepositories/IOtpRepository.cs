using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Domain.IRepositories
{
    public interface IOtpRepository
    {
        Task AddOtpAsync(Otp otp);
        Task<Otp> GetOtpByCodeAsync(string userId, string otpCode, OtpPurpose purpose);
        Task MarkOtpAsUsedAsync(Otp otp);
        Task UpdateOtpAsync(string userId, OtpPurpose purpose);
    }
}
