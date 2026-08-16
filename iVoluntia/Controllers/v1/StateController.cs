using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;
using Trustesse.Ivoluntia.Services.BusinessLogics.Service;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class StateController : BaseController
    {
        private readonly IStateService _stateService;
        public StateController(IStateService stateService)
        {
            _stateService = stateService;
        }
        [HttpPost("create-state")]
        public async Task<IActionResult> CreateState([FromBody] CreateStateModel request)
             => BuildHttpResponse<string>(await _stateService.CreateState(request));

        [HttpGet("get-country-states-by-countryid")]
        public async Task<IActionResult> GetStates([FromQuery] string countryId)
            => BuildHttpResponse<List<GetStateResponse>>(await _stateService.GetStates(countryId));

        [HttpGet("get-state-by-id")]
        public async Task<IActionResult> GetState([FromQuery] string stateId)
             => BuildHttpResponse<GetStateResponse>(await _stateService.GetState(stateId));

        [HttpDelete("delete-state")]
        public async Task<IActionResult> DeleteState([FromQuery] string id)
             => BuildHttpResponse<string>(await _stateService.DeleteState(id));
    }
}
