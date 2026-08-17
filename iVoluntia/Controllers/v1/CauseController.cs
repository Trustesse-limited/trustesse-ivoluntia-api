using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Cause;
using Trustesse.Ivoluntia.Commons.DTOs.Skill;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class CauseController : BaseController
    {
        private readonly ICauseService _causeService;
        public CauseController(ICauseService causeService)
        {
            _causeService = causeService;
        }
        [HttpPost("create-cause")]
        public async Task<IActionResult> CreateCause(CreateCauseRequestDto createCauseRequestDto)
          => BuildHttpResponse<string>(await _causeService.CreateCause(createCauseRequestDto));
        
        [HttpGet("get-all-causes")]
        public async Task<IActionResult> GetCause()
           => BuildHttpResponse<List<GetCauseResponseDto>>(await _causeService.GetCause());

        [HttpGet("get-cause-by-id")]
        public async Task<IActionResult> GetCauseId([FromQuery] string id)
          => BuildHttpResponse<GetCauseResponseDto>(await _causeService.GetCauseById(id));

        [HttpDelete("delete-cause-by-id")]
        public async Task<IActionResult> DeleteCause(string id)
          => BuildHttpResponse<string>(await _causeService.DeleteCause(id));
    }
}
