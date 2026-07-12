using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class SecurityQuestionRepository : GenericRepository<SecurityQuestion>, ISecurityQuestionRepository
    {
        private readonly iVoluntiaDataContext _context;

        public SecurityQuestionRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}



