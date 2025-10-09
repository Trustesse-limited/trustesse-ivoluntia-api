using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IProgramService
    {
        Task<ApiResponse<ProgramDto>> CreateProgram(CreateProgramDto data);
        Task<ApiResponse<bool>> RemoveProgram(string dataId);
        Task<ApiResponse<bool>> UpdateProgram(UpdateProgramDTO data);
        Task<ApiResponse<IEnumerable<ProgramDto>>> GetPrograms();
        Task<ApiResponse<IEnumerable<ProgramDto>>> GetProgram(string id);
    }
}
