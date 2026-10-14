using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs.Location;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<LocationService> _logger;
        public LocationService(IUnitOfWork uow, ILogger<LocationService> logger)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<GlobalRequestReponse<GetLocationResponseDto>> CreateLocation(CreateLocationRequestDto data)
        {
            try
            {
                var country = await _uow.countryRepo.GetByExpressionAsync(c => c.CountryName == data.Country);
                if (country == null)
                    return ResponseHelper.BuildResponse<GetLocationResponseDto>("Country doesn't exist", StatusCodes.Status404NotFound, null, false);

                var state = await _uow.stateRepo.GetByExpressionAsync(s => s.StateName == data.State);
                if (state == null)
                    return ResponseHelper.BuildResponse<GetLocationResponseDto>("State doesn't exist", StatusCodes.Status404NotFound, null, false);

                var location = new Location
                {
                    Address = data.Address,
                    City = data.City,
                    Zipcode = data.Zipcode,
                    CountryId = country.Id,
                    StateId = state.Id
                };

                await _uow.locationRepo.AddAsync(location);
                await _uow.CompleteAsync();

                var response = new GetLocationResponseDto
                {
                    Id = location.Id,
                    Address = location.Address,
                    City = location.City,
                    Zipcode = location.Zipcode,
                    CountryName = country.CountryName,
                    StateName = state.StateName
                };

                return ResponseHelper.BuildResponse("Location created successfully", StatusCodes.Status200OK, response, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseHelper.BuildResponse<GetLocationResponseDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
    }
}
