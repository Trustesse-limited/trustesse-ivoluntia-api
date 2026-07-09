using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.Models.Response;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface IFileUploadService
    {
        Task<GlobalRequestReponse<IReadOnlyList<string>>> UploadFilesAsync(IEnumerable<IFormFile> files);
        Task<string> UploadImageFromBase64Async(string base64String, string fileNameWithoutExtension, int maxFileSizeInMb = 5);
    }
}
