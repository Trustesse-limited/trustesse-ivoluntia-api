using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Infrastructure.Implementation
{
    public class CloudinaryService : IFileUploadService
    {
        private Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<ApiResponse<IReadOnlyList<string>>> UploadFilesAsync(IEnumerable<IFormFile> files)
        {
            if (files == null || !files.Any())
                return ApiResponse<IReadOnlyList<string>>
                    .Failure(StatusCodes.Status400BadRequest, "No files provided.");

            const long maxFileSize = 50 * 1024 * 1024; // 50MB

            try
            {
                var uploadTasks = files.Select(async file =>
                {
                    if (file == null)
                        throw new Exception("File is required");

                    if (file.Length == 0)
                        throw new Exception("Empty file not allowed");

                    if (file.Length > maxFileSize)
                        throw new Exception("File exceeds 50MB");

                    await using var stream = file.OpenReadStream();

                    var uploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "file_uploads",
                        UseFilename = true,
                        UniqueFilename = true
                    };

                    var result = await _cloudinary.UploadAsync(uploadParams);

                    if (result.Error != null)
                        throw new Exception(result.Error.Message);

                    return result.SecureUrl.AbsoluteUri;
                });

                var urls = await Task.WhenAll(uploadTasks);

                return ApiResponse<IReadOnlyList<string>>
                    .Success("Files uploaded successfully", urls);
            }
            catch (Exception ex)
            {
                return ApiResponse<IReadOnlyList<string>>
                    .Failure(StatusCodes.Status400BadRequest, ex.Message);
            }
        }
    }
}
