using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileUploadsController : BaseController
    {
        private readonly IFileUploadService _fileService;
        public FileUploadsController(IFileUploadService fileService)
        {
            _fileService = fileService;
        }

        [HttpPost("file-uploads")]
        public async Task<IActionResult> Upload(List<IFormFile> files)
            => BuildHttpResponse(await _fileService.UploadFilesAsync(files));

        [HttpPost("file-upload")]
        public async Task<IActionResult> UploadOne(IFormFile file)
            => BuildHttpResponse(await _fileService.UploadFilesAsync(new List<IFormFile> { file }));
    }
}
