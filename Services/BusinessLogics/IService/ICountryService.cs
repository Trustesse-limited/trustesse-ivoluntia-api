using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface ICountryService
    {
        Task<CustomResponse> AddCountry(Country country);
        Task<Country> GetCountryById(string countryId);
        Task<IReadOnlyList<Country>> GetCountries();
        Task<CustomResponse> CreateStateAsync(CreateStateModel model);
        Task<State> GetStateByIdAsync(string stateId);
        Task<CustomResponse> GetStatesByCountryIdAsync(string countryId);
    }
}
