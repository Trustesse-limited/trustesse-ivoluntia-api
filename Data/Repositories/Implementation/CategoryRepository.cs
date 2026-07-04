using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class CategoryRepository : GenericRepository<FoundationCategory>, ICategoryRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public CategoryRepository(iVoluntiaDataContext iVoluntiaDataContext): base(iVoluntiaDataContext) 
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}
