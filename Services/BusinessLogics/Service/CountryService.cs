using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.Repositories;
using Trustesse.Ivoluntia.Domain.Entities;
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
        public async Task<ApiResponse<string>> AddCountry(Country country)
        {
            try
            {
                var countryExist = await _uow.countryRepo.GetByExpressionAsync(x => x.CountryName.ToLower() == country.CountryName.ToLower());
                if(countryExist == null)
                {
                    _uow.countryRepo.Add(country);
                    await _uow.CompleteAsync();
                }
                return ApiResponse<string>.Success("Country Added successfully.", null);

            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
               
            }
        }
        public async Task<Country> GetCountryById(string countryId)
        {
            Country country = null;
            try
            {
                country = await _uow.countryRepo.GetByIdAsync(countryId);

            }
            catch (Exception ex)
            {

              
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
        public async Task<ApiResponse<string>> CreateStateAsync(CreateStateModel model)
        {
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
                                return ApiResponse<string>.Failure(StatusCodes.Status409Conflict, $"The state with the name {model.StateName} already exists.");
                            }
                            state = new State
                            {
                                CountryId = country.Id,
                                StateName = model.StateName
                            };
                            _uow.stateRepo.Add(state);
                            await _uow.CompleteAsync();
                        }
                        else
                        {
                            return ApiResponse<string>.Success($"country with{model.CountryId} deos not exist", null);
                        }
                    }
                }
                else
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request");
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
            return ApiResponse<string>.Success("State Save successfully.", null);
        }
        
        public async Task<State> GetStateByIdAsync(string stateId)
        {
            State result = null;
            try
            {
                result = await _uow.stateRepo.GetByIdAsync(stateId);
            }
            catch (Exception ex)
            {
               
            }
            return result;
        }
        public async Task<ApiResponse<List<GetStateResponse>>> GetStatesByCountryIdAsync(string countryId)
        {
            IEnumerable<State> states = null;
            var stateResponse = new List<GetStateResponse>();
            try
            {
                states = await _uow.stateRepo.GetStateByCountryId(countryId);
                if (states is not null)
                {
                    stateResponse = states.Select(x => new GetStateResponse
                    {
                        StateId = x.Id,
                        StateName = x.StateName,
                        CountryId = x.Country.Id,
                        CountryName = x.Country.CountryName
                    }).OrderBy(x => x.StateName).ToList();
                  
                    return ApiResponse<List<GetStateResponse>>.Success("State Successfully retreive", stateResponse); 
                }
                else
                {
                    return ApiResponse<List<GetStateResponse>>.Success("No State Found", stateResponse);
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<List<GetStateResponse>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
