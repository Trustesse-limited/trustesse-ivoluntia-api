using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
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
        public async Task<GlobalRequestReponse<FavoriteProgramDto>> AddFavoriteProgram(AddFavoriteProgramRequest request)
        {
            var userId = _currentUserService.GetUserId();

            if (userId == null)
                return ResponseHelper.BuildResponse<FavoriteProgramDto>("Invalid user", StatusCodes.Status400BadRequest, null, false);

            var program = await _uow.programRepo.GetByExpressionAsync(x => x.Id == request.ProgramId);

            if (program == null)
                return ResponseHelper.BuildResponse<FavoriteProgramDto>("Program not found", StatusCodes.Status404NotFound, null, false);

            var exists = await _uow.favoriteProgramRepo.GetByExpressionAsync(x => x.UserId == userId && x.ProgramId == request.ProgramId);

            if (exists != null)
                return ResponseHelper.BuildResponse<FavoriteProgramDto>("Program already in favorites", StatusCodes.Status400BadRequest, null, false);

            var favorite = new FavoriteProgram
            {
                UserId = userId,
                ProgramId = request.ProgramId,
                DateAdded = DateTime.UtcNow
            };

            await _uow.favoriteProgramRepo.AddAsync(favorite);

            await _uow.CompleteAsync();

            var resultDto = _mapper.Map<FavoriteProgramDto>(favorite);

            return ResponseHelper.BuildResponse("Favorite Program added successfully", StatusCodes.Status200OK, resultDto, true);
        }

        public async Task<GlobalRequestReponse<IEnumerable<FavoriteProgramDto>>> GetFavoritePrograms()
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var query = _uow.favoriteProgramRepo.GetListByExpressionAsync(x => x.UserId == userId);

                var response = await query;

                var resultDto = _mapper.Map<IEnumerable<FavoriteProgramDto>>(response);

                return ResponseHelper.BuildResponse("Favorite programs retrieved successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception)
            {
                return ResponseHelper.BuildResponse<IEnumerable<FavoriteProgramDto>>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
        public async Task<GlobalRequestReponse<PagedResponse<FavoriteProgramDto>>> GetAllFavoritePrograms(BaseQuery baseQuery)
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

                return ResponseHelper.BuildResponse("Favorite programs retrieved successfully", StatusCodes.Status200OK, pagedResult, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<PagedResponse<FavoriteProgramDto>>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<bool>> RemoveFavoriteProgram(string programId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                var favorite = await _uow.favoriteProgramRepo.GetByExpressionAsync(x => x.UserId == userId && x.ProgramId == programId);

                if (favorite == null)
                    return ResponseHelper.BuildResponse("Program not found", StatusCodes.Status404NotFound, false, false);

                await _uow.favoriteProgramRepo.DeleteAsync(favorite);

                await _uow.CompleteAsync();

                return ResponseHelper.BuildResponse("Favorite Program removed successfully", StatusCodes.Status200OK, true, true);
            }
            catch (Exception)
            {
                return ResponseHelper.BuildResponse("An error occurred", StatusCodes.Status500InternalServerError, false, false);
            }
        }
    }
}
