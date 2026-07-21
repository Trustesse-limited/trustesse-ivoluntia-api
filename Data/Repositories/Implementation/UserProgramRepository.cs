using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class UserProgramRepository : GenericRepository<UserProgram>, IUserProgramRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext1;
        public UserProgramRepository(iVoluntiaDataContext iVoluntiaDataContext): base(iVoluntiaDataContext)     
        {
            _iVoluntiaDataContext1 = iVoluntiaDataContext;  
        }
    }
}
