using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : BaseController
    {
        private readonly ISkillService _skillService;
        public SkillController(ISkillService skillService)
        {
            _skillService = skillService;
        }
        [HttpPost("create-skill")]
        public async Task<IActionResult> CreateSkill(CreateSkillRequestDto createSkillDto)
          => BuildHttpResponse<string>(await _skillService.CreateSkill(createSkillDto));

        [HttpGet("get-all-skill")]
        public async Task<IActionResult> GetSkill()
           => BuildHttpResponse<List<GetSkillResponseDto>> (await _skillService.GetSkill());

        [HttpGet("get-skill-by-id")]
        public async Task<IActionResult> GetSkillById([FromQuery] string id)
          => BuildHttpResponse<GetSkillResponseDto>(await _skillService.GetSkillById(id));

        [HttpDelete("delete-skill-by-id")]
        public async Task<IActionResult> DeleteSkill(string id)
          => BuildHttpResponse<string>(await _skillService.DeleteSkill(id));
    }
}
