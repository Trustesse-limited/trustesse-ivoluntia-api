using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[Controller]")]
    [ApiController]
    public class FileUploadsController : ControllerBase
    {
        private readonly IFileUploadService _fileService;
        public FileUploadsController(IFileUploadService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("file-upload")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            if (files == null || !files.Any())
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var response = await _fileService.UploadFilesAsync(files);

            return Ok(response);
        }
    }
}
