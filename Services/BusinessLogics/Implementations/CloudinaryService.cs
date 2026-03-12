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

        public async Task<string> UploadImageFromBase64Async(string base64String, string fileNameWithoutExtension, int maxFileSizeInMb = 5)
        {
            if (_cloudinary == null)
                throw new InvalidOperationException("Cloudinary not initialized. Call Initialize first.");

            string mimeType = "image/jpeg";
            string extension = ".jpg";

            if (base64String.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                var header = base64String.Substring(5, base64String.IndexOf(";") - 5);
                mimeType = header;

                var allowedMimeTypes = new HashSet<string>
                {
                    "image/jpeg",
                    "image/jpg",
                    "image/png",
                    "image/gif",
                    "image/webp"
                };

                if (!allowedMimeTypes.Contains(mimeType.ToLower()))
                {
                    throw new InvalidOperationException($"Unsupported file type: {mimeType}");
                }

                extension = mimeType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    "image/jpg" => ".jpg",
                    "image/gif" => ".gif",
                    "image/webp" => ".webp",
                    _ => ".jpg"
                };
            }

            var base64Data = base64String.Contains(",") ? base64String.Split(',')[1] : base64String;

            byte[] imageBytes = Convert.FromBase64String(base64Data);

            double fileSizeInMb = (double)imageBytes.Length / (1024 * 1024);

            if (fileSizeInMb > maxFileSizeInMb)
            {
                throw new InvalidOperationException($"File size exceeds {maxFileSizeInMb} MB limit (uploaded: {fileSizeInMb:F2} MB)");
            }

            using var stream = new MemoryStream(imageBytes);

            var fileName = $"{fileNameWithoutExtension}{extension}";

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(fileName, stream),
                Folder = "file_uploads",
                UseFilename = true,
                UniqueFilename = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            return uploadResult.SecureUrl?.AbsoluteUri;
        }
    }
}
