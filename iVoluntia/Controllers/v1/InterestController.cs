using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs.Interest;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterestController : BaseController
    {
        private readonly IInterestService _interestService;
        public InterestController(IInterestService interestService)
        {
            _interestService = interestService;
        }
        [HttpPost("create-interest")]
        public async Task<IActionResult> CreateInterest(CreateInterestRequestDto createInterestDto)
          => BuildHttpResponse<string>(await _interestService.CreateInterest(createInterestDto));

        [HttpGet("get-interests")]
        public async Task<IActionResult> GetInterest()
          => BuildHttpResponse<List<GetInterestResponseDto>>(await _interestService.GetInterest());

        [HttpGet("get-interest-by-id")]
        public async Task<IActionResult> GetInterestById(string id)
           => BuildHttpResponse<GetInterestResponseDto>(await _interestService.GetInterestById(id));

        [HttpDelete("delete-interest-by-id")]
        public async Task<IActionResult> DeleteInterest(string id)
          => BuildHttpResponse<string>(await _interestService.DeleteInterest(id));
    }
}
