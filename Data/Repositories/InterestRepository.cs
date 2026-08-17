using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class InterestRepository : GenericRepository<Interest>, IInterestRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public InterestRepository(iVoluntiaDataContext iVoluntiaDataContext) : base(iVoluntiaDataContext)
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}
