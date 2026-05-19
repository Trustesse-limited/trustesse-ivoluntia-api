using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteProgramsController : ControllerBase
    {
        private readonly IProgramService _programService;
        private readonly IFavoriteProgramService _favoriteProgramService;

        public FavoriteProgramsController(IProgramService programService, IFavoriteProgramService favoriteProgramService)
        {
            _programService = programService;
            _favoriteProgramService = favoriteProgramService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddFavoriteProgram([FromBody] AddFavoriteProgramRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var result = await _favoriteProgramService.AddFavoriteProgramAsync(request);

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

            var result = await _favoriteProgramService.RemoveFavoriteProgramAsync(programId);

            return Ok(result);
        }
    }
}
