using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IProgramService
    {
        Task<GlobalRequestReponse<ProgramDto>> CreateProgram(CreateProgramDto data);
        Task<GlobalRequestReponse<bool>> RemoveProgram(string dataId);
        Task<GlobalRequestReponse<bool>> UpdateProgram(UpdateProgramDTO data);
        Task<GlobalRequestReponse<IEnumerable<ProgramDto>>> GetPrograms();
        Task<GlobalRequestReponse<IEnumerable<ProgramDto>>> GetProgram(string id);
        Task<GlobalRequestReponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto);
        Task<GlobalRequestReponse<bool>> DeleteProgramGoals(string programGoalId);
        Task<GlobalRequestReponse<string>> JoinProgram(string programId);
        Task<GlobalRequestReponse<string>> LeaveProgram(string programId);
    }
}
