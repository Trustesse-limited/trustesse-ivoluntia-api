using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using static System.Net.WebRequestMethods;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class OtpRepository: IOtpRepository
	{

        private readonly iVoluntiaDataContext _dbContext;
        public OtpRepository(iVoluntiaDataContext context)
        {
            _dbContext = context;
        }

        public async Task AddOtpAsync(Otp otp)
        {
            await _dbContext.Otps.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<Otp> GetOtpByCodeAsync(string userId, string otpCode, OtpPurpose purpose)
        {
            return await _dbContext.Otps
                .Where(o => o.UserId == userId && o.OtpCode == otpCode && o.Purpose == purpose && !o.IsUsed).FirstOrDefaultAsync();
		}

        public async Task MarkOtpAsUsedAsync(Otp otp)
        {
            otp.IsUsed = true;
            _dbContext.Otps.Update(otp);
            await _dbContext.SaveChangesAsync();
		}

        public async Task UpdateOtpAsync(string userId, OtpPurpose purpose)
        {
            var existingOtps = await _dbContext.Otps
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in existingOtps)
            {
                otp.IsUsed = true;
			}

			_dbContext.Otps.UpdateRange(existingOtps);
			await _dbContext.SaveChangesAsync();
		}
    }
}
