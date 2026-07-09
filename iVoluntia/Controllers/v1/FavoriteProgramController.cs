using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteProgramsController : BaseController
    {
        private readonly IFavoriteProgramService _favoriteProgramService;

        public FavoriteProgramsController(IFavoriteProgramService favoriteProgramService)
        {
            _favoriteProgramService = favoriteProgramService;
        }

        [HttpPost("add-favorite-program")]
        public async Task<IActionResult> AddFavoriteProgram([FromBody] AddFavoriteProgramRequest request)
            => BuildHttpResponse(await _favoriteProgramService.AddFavoriteProgram(request.Validate()));

        [HttpGet("get-favorite-programs")]
        public async Task<IActionResult> GetFavoritePrograms()
            => BuildHttpResponse(await _favoriteProgramService.GetFavoritePrograms());

        [HttpDelete("remove-favorite-program")]
        public async Task<IActionResult> RemoveFavoriteProgram(string programId)
            => BuildHttpResponse(await _favoriteProgramService.RemoveFavoriteProgram(programId));

        [HttpGet("get-all-favorite-programs")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> GetAllFavoritePrograms([FromQuery] BaseQuery baseQuery)
            => BuildHttpResponse(await _favoriteProgramService.GetAllFavoritePrograms(baseQuery));
    }
}
