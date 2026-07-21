using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Domain.IRepositories
{
    public interface IOtpRepository : IGenericRepository<Otp>
    {
        Task AddOtpAsync(Otp otp);
        Task<Otp> GetOtpByCodeAsync(string otpCode, string otpPurpose);
        Task MarkOtpAsUsedAsync(Otp otp);
        Task UpdateOtpAsync(string userId, OtpPurpose purpose);
    }
}
