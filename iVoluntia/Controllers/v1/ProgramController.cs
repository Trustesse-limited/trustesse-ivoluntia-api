using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{

    [ApiController]
    [ApiVersion("1.0")]
    [Route("v{version:apiVersion}/[controller]")]
    public class ProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;
        public ProgramsController(IProgramService programService)
        {
            _programService = programService;
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateProgram([FromBody] CreateProgramDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _programService.CreateProgram(request);

            return Ok(result);
        }

        [HttpGet("get-programs")]
        public async Task<IActionResult> GetPrograms()
        {
            var result = await _programService.GetPrograms();

            return Ok(result);
        }

        [HttpGet("get-program-by-id")]
        public async Task<IActionResult> GetProgram(string id)
        {
            var result = await _programService.GetProgram(id);

            return Ok(result);
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateProgram([FromBody] UpdateProgramDTO request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _programService.UpdateProgram(request);

            return Ok(result);
        }


        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteProgram(string id)
        {
            if (id == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _programService.RemoveProgram(id);

            return Ok(result);
        }
        [HttpPut("update-program-status")]
        [Authorize(Roles = "SuperAdmin, FoundationAdmin")]
        public async Task<IActionResult> UpdateProgramStatusAsync([FromBody]UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                if(updateProgramStatusDto.ProgramId == null && updateProgramStatusDto.Status == null)
                {
                    return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "programid and status cannot be null"));
                }
                if (HttpContext.User.IsInRole("FoundationAdmin") & updateProgramStatusDto.Status != "Pending")
                {
                    return Unauthorized(ApiResponse<string>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized"));
                }
                var statusUpdate = await _programService.UpdateProgramStatusAsync(updateProgramStatusDto);
                if(statusUpdate.StatusCode == StatusCodes.Status200OK)
                {
                    return Ok(statusUpdate);
                }
                return BadRequest(statusUpdate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }
    }
}
