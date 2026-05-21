using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IFavoriteProgramRepository
    {
        Task<FavoriteProgram> AddFavoriteProgram(FavoriteProgram data);
        Task<bool> RemoveFavoriteProgram(string programId);
        IQueryable<FavoriteProgram> GetFavoritePrograms();
    }
}
