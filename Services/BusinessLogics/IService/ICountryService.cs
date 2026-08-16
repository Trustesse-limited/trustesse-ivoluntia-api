using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Country;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ICountryService
    {
        Task<GlobalRequestReponse<string>> CreateCountry(CreateCountryRequestDto createCountryRequestDto);
        Task<GlobalRequestReponse<GetCountryResponseDto>> GetCountryById(string countryId);
        Task<GlobalRequestReponse<List<GetCountryResponse>>> GetCountries();
        Task<GlobalRequestReponse<string>> DeleteCountry(string countryId);
    }
}
