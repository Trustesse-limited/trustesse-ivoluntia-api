using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class FavoriteProgramService : IFavoriteProgramService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<ProgramService> _logger;
        private readonly IProgramRepository _programRepository;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        public FavoriteProgramService(IProgramRepository programRepository, iVoluntiaDataContext context, ICurrentUserService currentUserService, ILogger<ProgramService> logger, IMapper mapper)
        {
            _programRepository = programRepository;
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<ApiResponse<ProgramDto>> AddFavoriteProgramAsync(AddFavoriteProgramRequest request)
        {
            var userId = _currentUserService.GetUserId();
            var program = await _programRepository.GetPrograms().FirstOrDefaultAsync(x => x.Id == request.ProgramId);

            if (program == null)
                return ApiResponse<ProgramDto>.Failure(StatusCodes.Status404NotFound, "Program not found");

            var exists = await _context.FavoritePrograms.AnyAsync(x => x.UserId == userId && x.ProgramId == request.ProgramId);

            if (exists)
                return ApiResponse<ProgramDto>.Failure(StatusCodes.Status400BadRequest, "Program already in favorites");

            var favorite = new FavoriteProgram
            {
                UserId = userId,
                ProgramId = request.ProgramId
            };

            await _context.FavoritePrograms.AddAsync(favorite);

            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<ProgramDto>(program);


            return ApiResponse<ProgramDto>.Success("Program added successfully", resultDto);
        }

        public async Task<ApiResponse<IEnumerable<ProgramDto>>> GetFavoritePrograms()
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var query = _context.FavoritePrograms
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Program)
                    .Select(x => x.Program);

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<ProgramDto>>(response);

                return ApiResponse<IEnumerable<ProgramDto>>.Success("Favorite programs retrieved successfully", resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return ApiResponse<IEnumerable<ProgramDto>>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> RemoveFavoriteProgramAsync(string programId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var favorite = await _context.FavoritePrograms.FirstOrDefaultAsync(x => x.UserId == userId && x.ProgramId == programId);

                if (favorite == null)
                {
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");
                }

                _context.FavoritePrograms.Remove(favorite);

                await _context.SaveChangesAsync();


                return ApiResponse<bool>.Success("Program removed successfully", true);
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }
    }
}
