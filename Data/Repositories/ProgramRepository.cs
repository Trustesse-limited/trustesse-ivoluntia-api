using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class ProgramRepository : GenericRepository<Program>, IProgramRepository
    {
        private readonly iVoluntiaDataContext _context;
        public ProgramRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}
