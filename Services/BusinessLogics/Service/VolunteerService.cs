using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Volunteer;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class VolunteerService : IVolunteerService
    {
        private readonly ILogger<VolunteerService> _logger;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public VolunteerService(
            ILogger<VolunteerService> logger,
            iVoluntiaDataContext context,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _logger = logger;
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _uow = uow;
        }

        public async Task<ApiResponse<IEnumerable<VolunteerDto>>> GetVolunteers(string foundationId, bool? isActive)
        {
            try
            {
                var foundationResult = await _uow.foundationRepo.GetByExpressionAsync(f => f.Id == foundationId);

                if (foundationResult == null)
                    return ApiResponse<IEnumerable<VolunteerDto>>.Failure(StatusCodes.Status404NotFound, "No foundation found for this id");

                var query = await _uow.volunteerRepo.GetListByExpressionAsync(f => f.FoundationId == foundationId && (!isActive.HasValue || f.IsActive == isActive.Value));

                var resultDto = _mapper.Map<IEnumerable<VolunteerDto>>(query);

                return ApiResponse<IEnumerable<VolunteerDto>>.Success("Volunteers retrieved successfully", resultDto);
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
