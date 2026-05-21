using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
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
        private readonly IFavoriteProgramRepository _favoriteProgramRepository;
        private readonly IProgramRepository _programRepository;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        public FavoriteProgramService(IFavoriteProgramRepository favoriteProgramRepository,
            IProgramRepository programRepository,
            iVoluntiaDataContext context,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _favoriteProgramRepository = favoriteProgramRepository;
            _programRepository = programRepository;
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }
        public async Task<ApiResponse<FavoriteProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request)
        {
            var userId = _currentUserService.GetUserId();

            if (userId == null)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status400BadRequest, "Invalid user");

            var program = await _programRepository.GetPrograms().FirstOrDefaultAsync(x => x.Id == request.ProgramId);

            if (program == null)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status404NotFound, "Program not found");

            var exists = await _context.FavoritePrograms.AnyAsync(x => x.UserId == userId && x.ProgramId == request.ProgramId);

            if (exists)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status400BadRequest, "Program already in favorites");

            var favorite = new FavoriteProgram
            {
                UserId = userId,
                ProgramId = request.ProgramId,
                DateAdded = DateTime.UtcNow
            };

            await _favoriteProgramRepository.AddFavoriteProgram(favorite);

            await _context.SaveChangesAsync();

            var resultDto = _mapper.Map<FavoriteProgramDto>(favorite);

            return ApiResponse<FavoriteProgramDto>.Success("Favorite Program added successfully", resultDto);
        }

        public async Task<ApiResponse<IEnumerable<FavoriteProgramDto>>> GetFavoritePrograms()
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var query = _favoriteProgramRepository.GetFavoritePrograms()
                    .Where(x => x.UserId == userId)
                    .Include(x => x.Program)
                    .Select(x => x.Program);

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<FavoriteProgramDto>>(response);

                return ApiResponse<IEnumerable<FavoriteProgramDto>>.Success("Favorite programs retrieved successfully", resultDto);
            }
            catch (Exception)
            {
                return ApiResponse<IEnumerable<FavoriteProgramDto>>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }
        public async Task<ApiResponse<PagedResponse<FavoriteProgramDto>>> GetAllFavoritePrograms(BaseQuery baseQuery)
        {
            try
            {
                var pageNumber = baseQuery.PageNumber;
                var pageSize = baseQuery.PageSize;

                pageNumber = pageNumber < 1 ? 1 : pageNumber;
                pageSize = pageSize < 1 ? 10 : pageSize;

                var query = _favoriteProgramRepository.GetFavoritePrograms()
                    .Select(x => x.Program);

                var totalCount = await query.CountAsync();

                var programs = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<FavoriteProgramDto>>(programs);

                var pagedResult = new PagedResponse<FavoriteProgramDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                    Data = resultDto
                };

                return ApiResponse<PagedResponse<FavoriteProgramDto>>
                    .Success("Favorite programs retrieved successfully", pagedResult);
            }
            catch (Exception)
            {
                return ApiResponse<PagedResponse<FavoriteProgramDto>>
                    .Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> RemoveFavoriteProgram(string programId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var favorite = await _favoriteProgramRepository.GetFavoritePrograms().FirstOrDefaultAsync(x => x.UserId == userId && x.ProgramId == programId);

                if (favorite == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");

                await _favoriteProgramRepository.RemoveFavoriteProgram(favorite.ProgramId);

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success("Program removed successfully", true);
            }
            catch (Exception)
            {
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }
    }
}
