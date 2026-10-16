using Trustesse.Ivoluntia.Commons.DTOs.Location;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ILocationService
    {
        Task<GlobalRequestReponse<GetLocationResponseDto>> CreateLocation(CreateLocationRequestDto data);
    }
}
