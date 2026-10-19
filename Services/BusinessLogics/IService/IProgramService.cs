using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IProgramService
    {
        Task<GlobalRequestReponse<ProgramDto>> CreateProgram(CreateProgramDto data);
        Task<GlobalRequestReponse<bool>> RemoveProgram(string dataId);
        Task<GlobalRequestReponse<bool>> UpdateProgram(UpdateProgramDTO data);
        Task<GlobalRequestReponse<PagedResponse<ProgramDto>>> GetPrograms(BaseQuery baseQuery, string? foundationId = null, string? status = null, string? search = null, string? dateRange = null);
        Task<GlobalRequestReponse<ProgramStatsDto>> GetProgramStats(string? foundationId = null);
        Task<GlobalRequestReponse<IEnumerable<ProgramDto>>> GetProgram(string id);
        Task<GlobalRequestReponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto);
        Task<GlobalRequestReponse<bool>> DeleteProgramGoals(string programGoalId);
        Task<GlobalRequestReponse<string>> JoinProgram(string programId);
        Task<GlobalRequestReponse<string>> LeaveProgram(string programId);
    }
}
