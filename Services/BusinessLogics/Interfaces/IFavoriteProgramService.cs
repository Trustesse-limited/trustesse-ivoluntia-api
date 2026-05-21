using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IFavoriteProgramService
    {
        Task<ApiResponse<ProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request);
        Task<ApiResponse<bool>> RemoveFavoriteProgram(string programId);
        Task<ApiResponse<IEnumerable<ProgramDto>>> GetFavoritePrograms();
    }
}
