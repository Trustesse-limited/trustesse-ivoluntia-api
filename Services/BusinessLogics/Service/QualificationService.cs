using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs.Qualification;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class QualificationService : IQualificationService
    {
        private readonly ILogger<QualificationService> _logger;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public QualificationService(ILogger<QualificationService> logger, IMapper mapper, IUnitOfWork uow)
        {
            _logger = logger;
            _mapper = mapper;
            _uow = uow;
        }

        public async Task<GlobalRequestReponse<QualificationDto>> CreateQualification(CreateQualificationDto request)
        {
            try
            {
                if (request == null)
                    return ResponseHelper.BuildResponse<QualificationDto>("Invalid request", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.Title))
                    return ResponseHelper.BuildResponse<QualificationDto>("Title is required", StatusCodes.Status400BadRequest, null, false);

                if (request.Title.Trim().Length > 100)
                    return ResponseHelper.BuildResponse<QualificationDto>("Title must not exceed 100 characters", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.SupportingDocumentFormat))
                    return ResponseHelper.BuildResponse<QualificationDto>("Supporting document format is required", StatusCodes.Status400BadRequest, null, false);

                if (request.SupportingDocumentFormat.Trim().Length > 20)
                    return ResponseHelper.BuildResponse<QualificationDto>(
                        "Supporting document format must not exceed 20 characters", StatusCodes.Status400BadRequest, null, false);

                if (request.SupportingDocumentMaxSize <= 0)
                    return ResponseHelper.BuildResponse<QualificationDto>("Supporting document max size must be greater than 0", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.SupportingDocumentFileSizeUnit) ||
                    !Enum.TryParse<FileSizeUnit>(request.SupportingDocumentFileSizeUnit, true, out var fileSizeUnit) ||
                    !Enum.IsDefined(typeof(FileSizeUnit), fileSizeUnit))
                    return ResponseHelper.BuildResponse<QualificationDto>(
                        "Supporting document file size unit must be one of: B, KB, MB, GB, TB", StatusCodes.Status400BadRequest, null, false);

                var normalizedTitle = request.Title.Trim().ToUpper();

                var existing = await _uow.qualificationRepo.GetByExpressionAsync(q => q.Title.Trim().ToUpper() == normalizedTitle);

                if (existing != null)
                    return ResponseHelper.BuildResponse<QualificationDto>("A qualification with this title already exists", StatusCodes.Status409Conflict, null, false);

                var qualification = new Qualification
                {
                    Title = request.Title.Trim(),
                    SupportingDocumentFormat = request.SupportingDocumentFormat.Trim(),
                    SupportingDocumentMaxSize = request.SupportingDocumentMaxSize,
                    SupportingDocumentFileSizeUnit = fileSizeUnit
                };

                await _uow.qualificationRepo.AddAsync(qualification);

                await _uow.CompleteAsync();

                var resultDto = _mapper.Map<QualificationDto>(qualification);

                return ResponseHelper.BuildResponse("Qualification created successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<QualificationDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<QualificationDto>> UpdateQualification(string qualificationId, UpdateQualificationDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qualificationId))
                    return ResponseHelper.BuildResponse<QualificationDto>("Qualification id is required", StatusCodes.Status400BadRequest, null, false);

                var qualification = await _uow.qualificationRepo.GetByIdAsync(qualificationId);

                if (qualification == null)
                    return ResponseHelper.BuildResponse<QualificationDto>("Qualification not found", StatusCodes.Status404NotFound, null, false);

                if (request == null)
                    return ResponseHelper.BuildResponse<QualificationDto>("Invalid request", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.Title))
                    return ResponseHelper.BuildResponse<QualificationDto>("Title is required", StatusCodes.Status400BadRequest, null, false);

                if (request.Title.Trim().Length > 100)
                    return ResponseHelper.BuildResponse<QualificationDto>("Title must not exceed 100 characters", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.SupportingDocumentFormat))
                    return ResponseHelper.BuildResponse<QualificationDto>("Supporting document format is required", StatusCodes.Status400BadRequest, null, false);

                if (request.SupportingDocumentFormat.Trim().Length > 20)
                    return ResponseHelper.BuildResponse<QualificationDto>(
                        "Supporting document format must not exceed 20 characters", StatusCodes.Status400BadRequest, null, false);

                if (request.SupportingDocumentMaxSize <= 0)
                    return ResponseHelper.BuildResponse<QualificationDto>("Supporting document max size must be greater than 0", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.SupportingDocumentFileSizeUnit) ||
                    !Enum.TryParse<FileSizeUnit>(request.SupportingDocumentFileSizeUnit, true, out var fileSizeUnit) ||
                    !Enum.IsDefined(typeof(FileSizeUnit), fileSizeUnit))
                    return ResponseHelper.BuildResponse<QualificationDto>(
                        "Supporting document file size unit must be one of: B, KB, MB, GB, TB", StatusCodes.Status400BadRequest, null, false);

                var normalizedTitle = request.Title.Trim().ToUpper();
                var normalizedExistingTitle = qualification.Title.Trim().ToUpper();

                if (normalizedTitle != normalizedExistingTitle)
                {
                    var duplicate = await _uow.qualificationRepo.GetByExpressionAsync(
                        q => q.Id != qualificationId && q.Title.Trim().ToUpper() == normalizedTitle);

                    if (duplicate != null)
                        return ResponseHelper.BuildResponse<QualificationDto>("A qualification with this title already exists", StatusCodes.Status409Conflict, null, false);
                }

                qualification.Title = request.Title.Trim();
                qualification.SupportingDocumentFormat = request.SupportingDocumentFormat.Trim();
                qualification.SupportingDocumentMaxSize = request.SupportingDocumentMaxSize;
                qualification.SupportingDocumentFileSizeUnit = fileSizeUnit;

                await _uow.qualificationRepo.UpdateAsync(qualification);

                await _uow.CompleteAsync();

                var resultDto = _mapper.Map<QualificationDto>(qualification);

                return ResponseHelper.BuildResponse("Qualification updated successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<QualificationDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<List<QualificationDto>>> GetAllQualifications()
        {
            try
            {
                var qualifications = await _uow.qualificationRepo.GetAsync(
                    orderby: q => q.OrderBy(x => x.Title),
                    pageNumber: 0,
                    pageSize: 0);

                var resultDtos = _mapper.Map<List<QualificationDto>>(qualifications);

                return ResponseHelper.BuildResponse("Qualifications retrieved successfully", StatusCodes.Status200OK, resultDtos, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<List<QualificationDto>>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<QualificationDto>> GetQualificationById(string qualificationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qualificationId))
                    return ResponseHelper.BuildResponse<QualificationDto>("Qualification id is required", StatusCodes.Status400BadRequest, null, false);

                var qualification = await _uow.qualificationRepo.GetByIdAsync(qualificationId);

                if (qualification == null)
                    return ResponseHelper.BuildResponse<QualificationDto>("Qualification not found", StatusCodes.Status404NotFound, null, false);

                var resultDto = _mapper.Map<QualificationDto>(qualification);

                return ResponseHelper.BuildResponse("Qualification retrieved successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<QualificationDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }

        public async Task<GlobalRequestReponse<string>> DeleteQualification(string qualificationId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(qualificationId))
                    return ResponseHelper.BuildResponse<string>("Qualification id is required", StatusCodes.Status400BadRequest, null, false);

                var qualification = await _uow.qualificationRepo.GetByIdAsync(qualificationId);

                if (qualification == null)
                    return ResponseHelper.BuildResponse<string>("Qualification not found", StatusCodes.Status404NotFound, null, false);

                var inUse = await _uow.userQualificationRepo.GetByExpressionAsync(uq => uq.QualificationTypeId == qualificationId);

                if (inUse != null)
                    return ResponseHelper.BuildResponse<string>("This qualification type is in use by one or more users and cannot be deleted", StatusCodes.Status409Conflict, null, false);

                qualification.IsDeprecated = true;

                await _uow.qualificationRepo.UpdateAsync(qualification);

                await _uow.CompleteAsync();

                return ResponseHelper.BuildResponse("Qualification type deleted successfully.", StatusCodes.Status200OK, qualificationId, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return ResponseHelper.BuildResponse<string>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
    }
}
