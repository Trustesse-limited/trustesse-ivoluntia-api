using System.Net;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1;
[Route("api/v1/countries")]
[ApiController]
public class CountryController : ControllerBase
{
    private readonly ICountryService _countryService;
    public CountryController(ICountryService  countryService)
    {
        _countryService = countryService;
    }
    
    [HttpPost("CreateCountry")]
    public async Task<IActionResult> CreateCountry([FromBody] Country request)
    {
        if (request == null)
            return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

        var result = await _countryService.AddCountry(request);

        if (result.ResponseCode != 200)
        {
            return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
        }

        return Ok(result);
    }
    
    [HttpPost("CreateState")]
    public async Task<IActionResult> CreateState([FromBody] CreateStateModel request)
    {
        if (request == null)
            return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

        var result = await _countryService.CreateStateAsync(request);

        if (result.ResponseCode != 200)
        {
            return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
        }

        return Ok(result);
    }
    
     [HttpGet("GetCountry")]
        public async Task<IActionResult> GetCountry(string countryId)
        {
            var result = await _countryService.GetCountryById(countryId);
            if (result is null)
            {

                return BadRequest(new { ResponseCode = (int)HttpStatusCode.BadRequest, ResponseMessage = "record not found" });
            }
            else
            {
                return Ok(new { ResponseCode = (int)HttpStatusCode.OK, ResponseMessage = "data returned successfully", Data = result });
            }

        }

        [HttpGet("GetCountries")]
        public async Task<IActionResult> GetCountries()
        {
            var result = await _countryService.GetCountries();
            if (result.Any())
            {
                    return Ok(new { ResponseCode = (int)HttpStatusCode.OK, ResponseMessage = "data returned", Data = result});
                
            }
            else
            {
                return BadRequest(new { ResponseCode = (int)HttpStatusCode.BadRequest, ResponseMessage = result});
            }

        }
        [HttpGet("GetState")]
        public async Task<IActionResult> GetState(string stateId)
        {
            var result = await _countryService.GetStateByIdAsync(stateId);
            if (result is null)
            {

                //return BadRequest(new { ResponseCode = (int)HttpStatusCode.BadRequest, ResponseMessage = "record not found" });
                return BadRequest(result);
            }
            else
            {
               // return Ok(new { ResponseCode = (int)HttpStatusCode.OK, ResponseMessage ="data returned successfully", Data = result });
                return Ok(result);
            }

        }
        [HttpGet("GetStates")]
        public async Task<IActionResult> GetStates(string countryId)
        {
            var result = await _countryService.GetStatesByCountryIdAsync(countryId);
            if (result.ResponseCode != 200)
            {
                return BadRequest(new { ResponseCode = 500, ResponseMessage = "Internal server error." });
            }
            else
            {
                return Ok(result);
            }

        }
}