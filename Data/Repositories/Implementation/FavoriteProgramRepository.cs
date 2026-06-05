using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class FavoriteProgramRepository : IFavoriteProgramRepository
    {
        private readonly iVoluntiaDataContext _context;

        public FavoriteProgramRepository(iVoluntiaDataContext context)
        {
            _context = context;
        }

        public async Task<FavoriteProgram> AddFavoriteProgram(FavoriteProgram data)
        {
            await _context.FavoritePrograms.AddAsync(data);
            return data;
        }

        public IQueryable<FavoriteProgram> GetFavoritePrograms()
        {
            var query = _context.FavoritePrograms
                    .Include(x => x.Program)
                    .AsQueryable();

            return query;
        }
        public IQueryable<FavoriteProgram> GetFavoriteProgramsByUserId(string userId)
        {
            var query = _context.FavoritePrograms
                .Where(x => x.UserId == userId)
                .Include(x => x.Program)
                .AsQueryable();

            return query;
        }

        public async Task<bool> RemoveFavoriteProgram(string dataId)
        {
            var data = await _context.FavoritePrograms.Where(p => p.Id == dataId).FirstOrDefaultAsync();

            _context.FavoritePrograms.Remove(data);

            return true;
        }
    }
}



