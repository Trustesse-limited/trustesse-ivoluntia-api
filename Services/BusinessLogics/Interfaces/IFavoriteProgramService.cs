using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IFavoriteProgramService
    {
        Task<ApiResponse<bool>> RemoveFavoriteProgram(string programId);
        Task<ApiResponse<FavoriteProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request);
        Task<ApiResponse<IEnumerable<FavoriteProgramDto>>> GetFavoritePrograms();
        Task<ApiResponse<PagedResponse<FavoriteProgramDto>>> GetAllFavoritePrograms(BaseQuery baseQuery);
    }
}
