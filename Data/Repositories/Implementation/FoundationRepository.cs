using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class FoundationRepository : IFoundationRepository
    {
        private readonly iVoluntiaDataContext _context;

        public FoundationRepository(iVoluntiaDataContext context)
        {
            _context = context;
        }
        public async Task<Foundation> CreateFoundation(Foundation data)
        {
            await _context.Foundations.AddAsync(data);
            return data;
        }


        public IQueryable<Foundation> GetFoundation(string dataId)
        {
            var query = _context.Foundations.AsQueryable();

            if (!string.IsNullOrEmpty(dataId))
                query = query.Where(p => p.Id == dataId);

            return query;
        }


        public IQueryable<Foundation> GetFoundations()
        {
            return _context.Foundations.AsQueryable();
        }


        public async Task<bool> RemoveFoundation(string dataId)
        {
            var data = await _context.Foundations.Where(p => p.Id == dataId).FirstAsync();

            _context.Foundations.Remove(data);

            return true;

        }

        public bool UpdateFoundation(Foundation data)
        {
            _context.Foundations.Update(data);

            return true;
        }
    }
}
