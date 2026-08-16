using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class FoundationRepository : GenericRepository<Foundation>, IFoundationRepository
    {
        private readonly iVoluntiaDataContext _context;

        public FoundationRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}
