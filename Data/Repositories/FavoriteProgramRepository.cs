using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class FavoriteProgramRepository : GenericRepository<FavoriteProgram>, IFavoriteProgramRepository
    {
        private readonly iVoluntiaDataContext _context;

        public FavoriteProgramRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}



