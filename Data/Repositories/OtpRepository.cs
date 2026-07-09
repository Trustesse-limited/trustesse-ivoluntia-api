using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class OtpRepository : GenericRepository<Otp>, IOtpRepository
    {
        private readonly iVoluntiaDataContext _context;
        public OtpRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}