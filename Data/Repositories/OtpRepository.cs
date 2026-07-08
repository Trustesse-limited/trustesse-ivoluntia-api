using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class OtpRepository : GenericRepository<Otp>, IOtpRepository
    {
        private readonly iVoluntiaDataContext _context;
        public OtpRepository(iVoluntiaDataContext context) : base(context)
    public class OtpRepository: IOtpRepository
	{
       
        private readonly iVoluntiaDataContext _dbContext;
        public OtpRepository(iVoluntiaDataContext context)
        {
            _context = context;
        }
            _dbContext = context;
        }

        public async Task AddOtpAsync(Otp otp)
        {
            await _dbContext.Otps.AddAsync(otp);
            await _dbContext.SaveChangesAsync();

        }

        public async Task<Otp> GetOtpByCodeAsync(string userId, string otpCode, OtpPurpose purpose)
        {
            //return await _dbContext.Otps
            //    .Where(o => o.UserId == userId && o.OtpCode == otpCode && o.Purpose == purpose && !o.IsUsed).FirstOrDefaultAsync();

            var resp = await _dbContext.Otps
                .Where(o => o.UserId == userId).FirstOrDefaultAsync();
            return resp;
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
                .Where(o => o.UserId == userId && o.Purpose == purpose.ToString() && !o.IsUsed)
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
