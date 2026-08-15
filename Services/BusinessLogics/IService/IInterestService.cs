using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IInterestService
    {
        Task<GlobalRequestReponse<string>> CreateInterest(CreateInterestRequestDto createInterestDto);
        Task<GlobalRequestReponse<List<GetInterestResponseDto>>> GetInterest();
        Task<GlobalRequestReponse<GetInterestResponseDto>> GetInterestById(string id);
        Task<GlobalRequestReponse<string>> DeleteInterest(string id);
    }
}
