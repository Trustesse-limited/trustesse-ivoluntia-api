using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IFavoriteProgramService
    {
        Task<GlobalRequestReponse<bool>> RemoveFavoriteProgram(string programId);
        Task<GlobalRequestReponse<FavoriteProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request);
        Task<GlobalRequestReponse<IEnumerable<FavoriteProgramDto>>> GetFavoritePrograms();
        Task<GlobalRequestReponse<PagedResponse<FavoriteProgramDto>>> GetAllFavoritePrograms(BaseQuery baseQuery);
    }
}
