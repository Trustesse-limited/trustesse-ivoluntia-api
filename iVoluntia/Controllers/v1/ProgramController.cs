using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[Controller]")]
    [ApiController]
    [Authorize]
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


        [HttpDelete("delete-program-goal")]
        public async Task<IActionResult> DeleteProgram(string programGoalId)
        {
            if (programGoalId == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _programService.DeleteProgramGoals(programGoalId);

            return Ok(result);
        }
        [HttpPut("update-program-status")]
        public async Task<IActionResult> UpdateProgramStatusAsync([FromBody]UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                if(updateProgramStatusDto.ProgramId == null && updateProgramStatusDto.Status == null)
                {
                    return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "program id and status cannot be null"));
                }
                if (HttpContext.User.IsInRole(UserRolesEnum.FoundationAdmin.ToString()) && updateProgramStatusDto.Status != ProgramStatus.Pending.ToString())
                {
                    return Unauthorized(ApiResponse<string>.Failure(StatusCodes.Status401Unauthorized, "Unauthorized"));
                }
                if (!HttpContext.User.IsInRole(UserRolesEnum.FoundationAdmin.ToString()) && !HttpContext.User.IsInRole(UserRolesEnum.SuperAdmin.ToString()))
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
                return BadRequest(ex.Message);
            }
        }
    }
}
