using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class UserSecurityQuestionRepository : GenericRepository<UserSecurityQuestion>, IUserSecurityQuestionRepository
    {
        private readonly iVoluntiaDataContext _context;

        public UserSecurityQuestionRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}



