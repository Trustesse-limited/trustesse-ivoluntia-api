using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs.Qualification;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
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

                if (request.SupportingDocumentMaxSize <= 0)
                    return ResponseHelper.BuildResponse<QualificationDto>("Supporting document max size must be greater than 0", StatusCodes.Status400BadRequest, null, false);

                if (string.IsNullOrWhiteSpace(request.SupportingDocumentFileSizeUnit) ||
                    !Enum.TryParse<FileSizeUnit>(request.SupportingDocumentFileSizeUnit, true, out var fileSizeUnit))
                    return ResponseHelper.BuildResponse<QualificationDto>(
                        "Supporting document file size unit must be one of: B, KB, MB, GB, TB", StatusCodes.Status400BadRequest, null, false);

                var normalizedTitle = request.Title.Trim().ToUpper();

                var existing = await _uow.qualificationRepo.GetByExpressionAsync(q => q.Title.ToUpper() == normalizedTitle);

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
    }
}
