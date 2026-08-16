using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Country;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public CountryService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<GlobalRequestReponse<string>> CreateCountry(CreateCountryRequestDto createCountryRequestDto)
        {
           var countryExist = await _uow.countryRepo.GetByExpressionAsync(x => x.CountryName.ToLower() == createCountryRequestDto.CountryName.ToLower());
           if(countryExist == null)
           {
              var mapCountry = _mapper.Map<Country>(createCountryRequestDto); 
              mapCountry.IsDeprecated = false;
              _uow.countryRepo.Add(mapCountry);
              var response = await _uow.CompleteAsync();
              if(response > 0)
                return ResponseHelper.BuildResponse<string>("success", StatusCodes.Status200OK, "country created", true);
                return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
            }
            return ResponseHelper.BuildResponse<string>("country already exist", StatusCodes.Status200OK, null, false);
        }
        public async Task<GlobalRequestReponse<GetCountryResponseDto>> GetCountryById(string countryId)
        {
            if(countryId == null)
                return ResponseHelper.BuildResponse<GetCountryResponseDto>("invalid request", StatusCodes.Status400BadRequest, null, false);
            var country = await _uow.countryRepo.GetByIdAsync(countryId);
            var mapCountry = _mapper.Map<GetCountryResponseDto>(country);
            return ResponseHelper.BuildResponse<GetCountryResponseDto>("success", StatusCodes.Status200OK, mapCountry, true);
        }
        public async Task<GlobalRequestReponse<List<GetCountryResponse>>> GetCountries()
        {
            var country = await _uow.countryRepo.GetAllAsync();
            if(country == null)
                return ResponseHelper.BuildResponse<List<GetCountryResponse>> ("something went wrong", StatusCodes.Status400BadRequest, null, false);
            var mapCountry = _mapper.Map<List<GetCountryResponse>>(country);
            return ResponseHelper.BuildResponse<List<GetCountryResponse>>("success", StatusCodes.Status200OK, mapCountry, true);
        }
        public async Task<GlobalRequestReponse<string>> DeleteCountry(string countryId)
        {
            if (countryId == null)
                return ResponseHelper.BuildResponse<string>("invalid request", StatusCodes.Status400BadRequest, null, false);
            var country = await _uow.countryRepo.GetByIdAsync(countryId); 
            await _uow.countryRepo.DeleteAsync(country);
            var response = await _uow.CompleteAsync();
            if(response > 0)
                return ResponseHelper.BuildResponse<string>("success", StatusCodes.Status200OK, "country deleted", true);
            return ResponseHelper.BuildResponse<string>("something went wrong", StatusCodes.Status400BadRequest, null, false);
        }
    }
}
