using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public interface IFavoriteProgramRepository : IGenericRepository<FavoriteProgram>
    {
        //Task<FavoriteProgram> AddFavoriteProgram(FavoriteProgram data);
        //Task<bool> RemoveFavoriteProgram(string programId);
        //IQueryable<FavoriteProgram> GetFavoritePrograms();
        //IQueryable<FavoriteProgram> GetFavoriteProgramsByUserId(string userId);
    }
}
