using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs.UserQualification;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class UserQualificationService : IUserQualificationService
    {
        private readonly ILogger<UserQualificationService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileUploadService _fileUploadService;

        public UserQualificationService(
            ILogger<UserQualificationService> logger,
            IMapper mapper,
            IUnitOfWork uow,
            ICurrentUserService currentUserService,
            IFileUploadService fileUploadService)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
            _currentUserService = currentUserService;
            _fileUploadService = fileUploadService;
        }

        public async Task<GlobalRequestReponse<UserQualificationDto>> AddUserQualification(CreateUserQualificationRequestDto request)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (string.IsNullOrWhiteSpace(userId))
                    return ResponseHelper.BuildResponse<UserQualificationDto>("Invalid user", StatusCodes.Status400BadRequest, null, false);

                if (request == null || string.IsNullOrWhiteSpace(request.QualificationTypeId))
                    return ResponseHelper.BuildResponse<UserQualificationDto>("Qualification type id is required", StatusCodes.Status400BadRequest, null, false);

                var qualificationType = await _uow.qualificationRepo.GetByIdAsync(request.QualificationTypeId);

                if (qualificationType == null)
                    return ResponseHelper.BuildResponse<UserQualificationDto>("Qualification type not found", StatusCodes.Status404NotFound, null, false);

                var duplicate = await _uow.userQualificationRepo.GetByExpressionAsync(
                    uq => uq.UserId == userId && uq.QualificationTypeId == request.QualificationTypeId);

                if (duplicate != null)
                    return ResponseHelper.BuildResponse<UserQualificationDto>("You already have this qualification type on your profile", StatusCodes.Status409Conflict, null, false);

                if (request.ProofOfQualification == null || request.ProofOfQualification.Length == 0)
                    return ResponseHelper.BuildResponse<UserQualificationDto>("Proof of qualification is required", StatusCodes.Status400BadRequest, null, false);

                var extension = Path.GetExtension(request.ProofOfQualification.FileName).TrimStart('.');

                if (!string.Equals(extension, qualificationType.SupportingDocumentFormat, StringComparison.OrdinalIgnoreCase))
                    return ResponseHelper.BuildResponse<UserQualificationDto>(
                        $"Proof of qualification must be a {qualificationType.SupportingDocumentFormat} file", StatusCodes.Status400BadRequest, null, false);

                var maxSizeInBytes = ToBytes(qualificationType.SupportingDocumentMaxSize, qualificationType.SupportingDocumentFileSizeUnit);

                if (request.ProofOfQualification.Length > maxSizeInBytes)
                    return ResponseHelper.BuildResponse<UserQualificationDto>(
                        $"Proof of qualification must not exceed {qualificationType.SupportingDocumentMaxSize}{qualificationType.SupportingDocumentFileSizeUnit}", StatusCodes.Status400BadRequest, null, false);

                var uploadResult = await _fileUploadService.UploadFilesAsync(new List<IFormFile> { request.ProofOfQualification });

                if (!uploadResult.isSuccessfull || uploadResult.Data == null || !uploadResult.Data.Any())
                    return ResponseHelper.BuildResponse<UserQualificationDto>(
                        uploadResult.Message ?? "Failed to upload proof of qualification", StatusCodes.Status400BadRequest, null, false);

                var userQualification = new UserQualification
                {
                    UserId = userId,
                    QualificationTypeId = request.QualificationTypeId,
                    ProofOfQualificationURL = uploadResult.Data.First()
                };

                await _uow.userQualificationRepo.AddAsync(userQualification);

                await _uow.CompleteAsync();

                var resultDto = _mapper.Map<UserQualificationDto>(userQualification);

                return ResponseHelper.BuildResponse("Qualification added successfully.", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<UserQualificationDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }

        private static long ToBytes(int size, FileSizeUnit unit)
        {
            long multiplier = unit switch
            {
                FileSizeUnit.B => 1L,
                FileSizeUnit.KB => 1024L,
                FileSizeUnit.MB => 1024L * 1024L,
                FileSizeUnit.GB => 1024L * 1024L * 1024L,
                FileSizeUnit.TB => 1024L * 1024L * 1024L * 1024L,
                _ => 1L
            };

            return size * multiplier;
        }
    }
}
