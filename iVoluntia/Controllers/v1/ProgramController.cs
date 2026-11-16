using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Domain.Entities;
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
        public async Task<IActionResult> GetPrograms([FromQuery] BaseQuery baseQuery, [FromQuery] string? foundationId, [FromQuery] string? status, [FromQuery] string? search, [FromQuery] string? dateRange)
            => BuildHttpResponse(await _programService.GetPrograms(baseQuery, foundationId, status, search, dateRange));

        [HttpGet("get-program-stats")]
        public async Task<IActionResult> GetProgramStats([FromQuery] string? foundationId)
            => BuildHttpResponse(await _programService.GetProgramStats(foundationId));

        [HttpGet("get-program-by-id")]
        public async Task<IActionResult> GetProgram(string id)
            => BuildHttpResponse(await _programService.GetProgram(id));

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProgram([FromBody] UpdateProgramDTO request)
            => BuildHttpResponse(await _programService.UpdateProgram(request.Validate()));

        [HttpDelete("delete-program-goal")]
        public async Task<IActionResult> DeleteProgramGoal(string programGoalId)
            => BuildHttpResponse(await _programService.DeleteProgramGoals(programGoalId));

        [HttpDelete("delete-program")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> RemoveProgram(string programId)
            => BuildHttpResponse(await _programService.RemoveProgram(programId));

        [HttpPut("updateprogramstatus")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> UpdateProgramStatusAsync([FromBody] UpdateProgramStatusDto updateProgramStatusDto)
            => BuildHttpResponse(await _programService.UpdateProgramStatusAsync(updateProgramStatusDto.Validate()));
        
        [HttpPost("join-program")]
        public async Task<IActionResult> JoinProgram(string programId, bool requireMeetUp, string? volunteeraAvailableTimeSlot)
            => BuildHttpResponse(await _programService.JoinProgram(programId, requireMeetUp, volunteeraAvailableTimeSlot));

        [HttpPost("leave-program")]
        public async Task<IActionResult> LeaveProgram(string programId)
            => BuildHttpResponse(await _programService.LeaveProgram(programId));

        [HttpPost("join-program-approve")]
        [Authorize(Roles = AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> JoinProgramApprove(string programId, string volunteerEmail)
            => BuildHttpResponse(await _programService.JoinProgramApprove(programId, volunteerEmail));

        [HttpPost("join-program-decline")]
        [Authorize(Roles = AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> JoinProgramDecline(string programId, string volunteerEmail, string declineReason)
            => BuildHttpResponse(await _programService.JoinProgramDecline(programId, volunteerEmail, declineReason));
    }
}