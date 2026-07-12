using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProgramsController : BaseController
    {
        private readonly IProgramService _programService;
        public ProgramsController(IProgramService programService)
        {
            _programService = programService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto request)
            => BuildHttpResponse(await _programService.CreateProgram(request.Validate()));

        [HttpGet("get-programs")]
        public async Task<IActionResult> GetPrograms()
            => BuildHttpResponse(await _programService.GetPrograms());

        [HttpGet("get-program-by-id")]
        public async Task<IActionResult> GetProgram(string id)
            => BuildHttpResponse(await _programService.GetProgram(id));

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProgram([FromBody] UpdateProgramDTO request)
            => BuildHttpResponse(await _programService.UpdateProgram(request.Validate()));

        [HttpDelete("delete-program-goal")]
        public async Task<IActionResult> DeleteProgram(string programGoalId)
            => BuildHttpResponse(await _programService.DeleteProgramGoals(programGoalId));

        [HttpPut("updateprogramstatus")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> UpdateProgramStatusAsync([FromBody] UpdateProgramStatusDto updateProgramStatusDto)
        {
            updateProgramStatusDto = updateProgramStatusDto.Validate();

            if (HttpContext.User.IsInRole(UserRolesEnum.FoundationAdmin.ToString()) && updateProgramStatusDto.Status != ProgramStatus.Pending.ToString())
                return Unauthorized(ResponseHelper.BuildResponse<string>("A foundation admin can only set a program's status to Pending", StatusCodes.Status401Unauthorized, null, false));

            return BuildHttpResponse(await _programService.UpdateProgramStatusAsync(updateProgramStatusDto));
        }

        [HttpPost("join-program")]
        public async Task<IActionResult> JoinProgram(string programId)
            => BuildHttpResponse(await _programService.JoinProgram(programId));

        [HttpPost("leave-program")]
        public async Task<IActionResult> LeaveProgram(string programId)
            => BuildHttpResponse(await _programService.LeaveProgram(programId));
    }
}
