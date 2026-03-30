using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class VolunteerService : IVolunteerService
    {
        private readonly ILogger<VolunteerService> _logger;
        private readonly iVoluntiaDataContext _context;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IFoundationRepository _foundationRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public VolunteerService(
            ILogger<VolunteerService> logger,
            iVoluntiaDataContext context,
            IVolunteerRepository volunteerRepository,
            IFoundationRepository foundationRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _volunteerRepository = volunteerRepository;
            _foundationRepository = foundationRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        //public async Task<ApiResponse<IEnumerable<VolunteerDto>>> GetVolunteers(string foundationId)
        //{
        //    try
        //    {
        //        var foundationResult = _foundationRepository.GetFoundation(foundationId);

        //        if (foundationResult == null)
        //            return ApiResponse<IEnumerable<VolunteerDto>>.Failure(StatusCodes.Status404NotFound, "No foundation found for this id");

        //        var query = _volunteerRepository.GetVolunteers(foundationId);

        //        var response = await query.ToListAsync();

        //        var resultDto = _mapper.Map<IEnumerable<VolunteerDto>>(response);

        //        return ApiResponse<IEnumerable<VolunteerDto>>.Success("Volunteers retrieved successfully", resultDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);

        //        return ApiResponse<IEnumerable<VolunteerDto>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred");
        //    }
        //}

        public async Task<ApiResponse<IEnumerable<VolunteerDto>>> GetVolunteers(string foundationId, bool? isActive)
        {
            try
            {
                var foundationResult = _foundationRepository.GetFoundation(foundationId);

                if (foundationResult == null)
                    return ApiResponse<IEnumerable<VolunteerDto>>.Failure(
                        StatusCodes.Status404NotFound,
                        "No foundation found for this id");

                var query = _volunteerRepository.GetVolunteers(foundationId, isActive);

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<VolunteerDto>>(response);

                return ApiResponse<IEnumerable<VolunteerDto>>.Success(
                    "Volunteers retrieved successfully",
                    resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return ApiResponse<IEnumerable<VolunteerDto>>.Failure(
                    StatusCodes.Status500InternalServerError,
                    "An error occurred");
            }
        }
    }
}
