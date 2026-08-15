using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ICauseService
    {
        Task<GlobalRequestReponse<string>> CreateCause(CreateCauseRequestDto createCauseRequestDto);
        Task<GlobalRequestReponse<List<GetCauseResponseDto>>> GetCause();
        Task<GlobalRequestReponse<GetCauseResponseDto>> GetCauseById(string id);
        Task<GlobalRequestReponse<string>> DeleteCause(string id);
    }
}
