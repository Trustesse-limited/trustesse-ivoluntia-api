using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IProgramRepository
    {
        Task<Program> CreateProgram(Program data);
        Task<bool> RemoveProgram(string dataId);
        IQueryable<Program> GetPrograms();
        bool UpdateProgram(Program data);
        IQueryable<Program> GetProgram(string dataId);
        Task<ApiResponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto);
    }
}
