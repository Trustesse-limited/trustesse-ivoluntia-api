using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly iVoluntiaDataContext _context;

        public ProgramRepository(iVoluntiaDataContext context)
        {
            _context = context;
        }
        public async Task<Program> CreateProgram(Program data)
        {
            await _context.Programs.AddAsync(data);
            return data;
        }


        public IQueryable<Program> GetProgram(string dataId)
        {
            var query = _context.Programs
                .Include(p => p.ProgramGoals)
                .Include(p => p.ProgramSkills)
                    .ThenInclude(ps => ps.Skill)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dataId))
                query = query.Where(p => p.Id == dataId);

            return query;
        }


        public IQueryable<Program> GetPrograms()
        {
            return _context.Programs.AsQueryable();
        }


        public async Task<bool> RemoveProgram(string dataId)
        {
            var data = await _context.Programs.Where(p => p.Id == dataId).FirstAsync();

            _context.Programs.Remove(data);

            return true;

        }

        public bool UpdateProgram(Program data)
        {
            _context.Programs.Update(data);

            return true;
        }
    }
}
