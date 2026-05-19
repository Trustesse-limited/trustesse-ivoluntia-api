using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IFavoriteProgramService
    {
        Task<ApiResponse<ProgramDto>> AddFavoriteProgramAsync(AddFavoriteProgramRequest request);
        Task<ApiResponse<bool>> RemoveFavoriteProgramAsync(string programId);
        Task<ApiResponse<IEnumerable<ProgramDto>>> GetFavoritePrograms();
    }
}
