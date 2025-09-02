using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ICountryService
    {
        Task<ApiResponse<string>> AddCountry(CreateCountryModel country);
        Task<Country> GetCountryById(Guid countryId);
        //Task<IReadOnlyList<Country>> GetCountries();
        Task<ApiResponse<List<GetCountryResponse>>> GetCountries();
        Task<ApiResponse<string>> CreateStateAsync(CreateStateModel model);
        Task<State> GetStateByIdAsync(Guid stateId);
        Task<ApiResponse<List<GetStateResponse>>>  GetStatesByCountryIdAsync(Guid countryId);
    }
}
