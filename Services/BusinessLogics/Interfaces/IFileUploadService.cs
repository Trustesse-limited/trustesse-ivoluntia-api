using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface IFileUploadService
    {
        Task<ApiResponse<IReadOnlyList<string>>> UploadFilesAsync(IEnumerable<IFormFile> files);
    }
}
