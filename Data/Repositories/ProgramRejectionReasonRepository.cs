using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class ProgramRejectionReasonRepository: GenericRepository<ProgramRejectionReason>, IProgramRejectionReasonRepository
    {
        private readonly iVoluntiaDataContext _iVoluntiaDataContext;
        public ProgramRejectionReasonRepository(iVoluntiaDataContext iVoluntiaDataContext):base(iVoluntiaDataContext) 
        {
            _iVoluntiaDataContext = iVoluntiaDataContext;
        }
    }
}
