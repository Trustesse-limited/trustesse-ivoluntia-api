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
    public class ProgramGoalRepository : GenericRepository<ProgramGoal>, IProgramGoalRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public ProgramGoalRepository(iVoluntiaDataContext iVoluntiaDataContext): base(iVoluntiaDataContext)       
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}
