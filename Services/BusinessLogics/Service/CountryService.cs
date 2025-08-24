using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _uow;
        public CountryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task AddCountry(Country country)
        {
            try
            {
                var countryExist = await _uow.countryRepo.GetByExpressionAsync(x => x.CountryName.ToLower() == country.CountryName.ToLower());
                if (countryExist == null)
                {
                    _uow.countryRepo.Add(country);
                    await _uow.CompleteAsync();
                }

            }
            catch (Exception ex)
            {


            }
        }

        public async Task<Country> GetCountryById(Guid countryId)
        {
            Country country = null;
            try
            {
                country = await _uow.countryRepo.GetByIdAsync(countryId);

            }
            catch (Exception ex)
            {

                throw;
            }
            return country;
        }

        public async Task<IReadOnlyList<Country>> GetCountries()
        {
            IReadOnlyList<Country> countries = new List<Country>();
            try
            {
                countries = await _uow.countryRepo.GetAllAsync();
            }
            catch (Exception ex)
            {


            }
            return countries;
        }

        public async Task<CustomResponse> CreateStateAsync(CreateStateModel model)
        {
            CustomResponse result = null;
            try
            {
                if (model != null)
                {
                    var country = await GetCountryById(model.CountryId);
                    {
                        if (country != null)
                        {
                            var state = await _uow.stateRepo.GetByExpressionAsync(x => x.StateName.ToLower() == model.StateName.ToLower());
                            if (state != null)
                            {
                                return new CustomResponse(400, "State already exist");
                            }
                            state = new State
                            {
                                CountryId = country.Id,
                                StateName = model.StateName
                            };
                            _uow.stateRepo.Add(state);
                            if (await _uow.CompleteAsync() > 0)
                            {
                                result = new CustomResponse(200, "State save successfully");
                            }
                        }
                        else
                        {
                            result = new CustomResponse(404, $"country with{model.CountryId} deos not exist");
                        }
                    }


                }
                else
                {
                    result = new CustomResponse(400, "Ivalid request object");
                }

            }
            catch (Exception ex)
            {


            }
            return result;
        }
    }
}
