using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Contants;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteProgramsController : ControllerBase
    {
        private readonly IFavoriteProgramService _favoriteProgramService;

        public FavoriteProgramsController(IFavoriteProgramService favoriteProgramService)
        {
            _favoriteProgramService = favoriteProgramService;
        }

        [HttpPost("add-favorite-program")]
        public async Task<IActionResult> AddFavoriteProgram([FromBody] AddFavoriteProgramRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _favoriteProgramService.AddFavoriteProgram(request);

            return Ok(result);
        }

        [HttpGet("get-favorite-programs")]
        public async Task<IActionResult> GetFavoritePrograms()
        {
            var result = await _favoriteProgramService.GetFavoritePrograms();

            return Ok(result);
        }

        [HttpDelete("remove-favorite-program")]
        public async Task<IActionResult> RemoveFavoriteProgram(string programId)
        {
            if (programId == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _favoriteProgramService.RemoveFavoriteProgram(programId);

            return Ok(result);
        }

        [HttpGet("get-all-favorite-programs")]
        [Authorize(Roles = AuthenticationConstants.SuperAdmin + "," + AuthenticationConstants.FoundationAdmin)]
        public async Task<IActionResult> GetAllFavoritePrograms([FromQuery] BaseQuery baseQuery)
        {
            var result = await _favoriteProgramService.GetAllFavoritePrograms(baseQuery);

            return Ok(result);
        }
    }
}
