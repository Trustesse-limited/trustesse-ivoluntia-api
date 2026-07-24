using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class UserSecurityValidationAttemptRepository : GenericRepository<UserSecurityValidationAttempt>, IUserSecurityValidationAttemptRepository
    {
        private readonly iVoluntiaDataContext _context;

        public UserSecurityValidationAttemptRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}



