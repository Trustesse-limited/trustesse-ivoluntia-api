using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class FavoriteProgramService : IFavoriteProgramService
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly iVoluntiaDataContext _context;
        private readonly ICurrentUserService _currentUserService;
        public FavoriteProgramService(
            iVoluntiaDataContext context,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IUnitOfWork uow)
        {
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _uow = uow;
        }
        public async Task<ApiResponse<FavoriteProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request)
        {
            var userId = _currentUserService.GetUserId();

            if (userId == null)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status400BadRequest, "Invalid user");

            var program = await _uow.favoriteProgramRepo.GetByExpressionAsync(x => x.Id == request.ProgramId);

            if (program == null)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status404NotFound, "Program not found");

            var exists = await _uow.favoriteProgramRepo.GetByExpressionAsync(x => x.UserId == userId && x.ProgramId == request.ProgramId);

            if (exists != null)
                return ApiResponse<FavoriteProgramDto>.Failure(StatusCodes.Status400BadRequest, "Program already in favorites");

            var favorite = new FavoriteProgram
            {
                UserId = userId,
                ProgramId = request.ProgramId,
                DateAdded = DateTime.UtcNow
            };

            await _uow.favoriteProgramRepo.AddAsync(favorite);

            await _uow.CompleteAsync();

            var resultDto = _mapper.Map<FavoriteProgramDto>(favorite);

            return ApiResponse<FavoriteProgramDto>.Success("Favorite Program added successfully", resultDto);
        }

        public async Task<ApiResponse<IEnumerable<FavoriteProgramDto>>> GetFavoritePrograms()
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var query = _uow.favoriteProgramRepo.GetListByExpressionAsync(x => x.UserId == userId);

                var response = await query;

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
                var (programs, totalCount) = await _uow.favoriteProgramRepo.GetPagedAsync(pageNumber: baseQuery.PageNumber, pageSize: baseQuery.PageSize);

                var resultDto = _mapper.Map<IEnumerable<FavoriteProgramDto>>(programs);

                var pagedResult = new PagedResponse<FavoriteProgramDto>
                {
                    PageNumber = baseQuery.PageNumber,
                    PageSize = baseQuery.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling((double)totalCount / baseQuery.PageSize),
                    Data = resultDto
                };

                return ApiResponse<PagedResponse<FavoriteProgramDto>>
                    .Success("Favorite programs retrieved successfully", pagedResult);
            }
            catch (Exception ex)
            {
                return ApiResponse<PagedResponse<FavoriteProgramDto>>
                    .Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> RemoveFavoriteProgram(string programId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var favorite = await _uow.favoriteProgramRepo.GetByExpressionAsync(x => x.UserId == userId && x.ProgramId == programId);

                if (favorite == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");

                await _uow.favoriteProgramRepo.DeleteAsync(favorite);

                await _uow.CompleteAsync();

                return ApiResponse<bool>.Success("Program removed successfully", true);
            }
            catch (Exception)
            {
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }
    }
}
