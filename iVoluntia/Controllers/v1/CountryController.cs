using System.Net;
using Azure.Core;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Country;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1;
[Route("api/v1/countries")]
[ApiController]
public class CountryController : BaseController
{
    private readonly ICountryService _countryService;
    public CountryController(ICountryService  countryService)
    {
        _countryService = countryService;
    }

    [HttpPost("create-country")]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequestDto createCountryRequestDto)
        => BuildHttpResponse<string>(await _countryService.CreateCountry(createCountryRequestDto));
   
    [HttpGet("get-country-by-id")]
    public async Task<IActionResult> GetCountryById([FromQuery] string countryId)
       => BuildHttpResponse<GetCountryResponseDto>(await _countryService.GetCountryById(countryId));

    [HttpGet("get-all-countries")]
    public async Task<IActionResult> GetCountries()
       => BuildHttpResponse<List<GetCountryResponse>>(await _countryService.GetCountries());
     
    [HttpDelete("delete-country-by-id")]
    public async Task<IActionResult> DeleteCountry([FromQuery] string countryId)
        => BuildHttpResponse<string>(await _countryService.DeleteCountry(countryId));
}