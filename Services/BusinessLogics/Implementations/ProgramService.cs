using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class ProgramService : IProgramService
    {
        private readonly ILogger<ProgramService> _logger;
        private readonly iVoluntiaDataContext _context;
        private readonly IProgramRepository _programRepository;
        private readonly IFoundationRepository _foundationRepository;
        private readonly IMapper _mapper;
        public ProgramService(
            ILogger<ProgramService> logger,
            iVoluntiaDataContext context,
            IProgramRepository programRepository,
            IFoundationRepository foundationRepository,
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _programRepository = programRepository;
            _foundationRepository = foundationRepository;
            _mapper = mapper;
        }
        public async Task<ApiResponse<ProgramDto>> CreateProgram(CreateProgramDto data)
        {
            try
            {
                var foundationExists = _foundationRepository.GetFoundation(data.FoundationId);

                if (foundationExists == null)
                    return ApiResponse<ProgramDto>.Failure(StatusCodes.Status404NotFound, "Foundation not found");

                var newData = _mapper.Map<Program>(data);

                if (data.SkillIds != null && data.SkillIds.Any())
                {
                    foreach (var skillId in data.SkillIds)
                    {
                        newData.ProgramSkills.Add(new ProgramSkill
                        {
                            SkillId = skillId,
                            ProgramId = newData.Id
                        });
                    }
                }

                if (data.ProgramGoals != null && data.ProgramGoals.Any())
                {
                    foreach (var goalDto in data.ProgramGoals)
                    {
                        newData.ProgramGoals.Add(new ProgramGoal
                        {
                            Goal = goalDto.Goal,
                            IsAchieved = false
                        });
                    }
                }

                newData.IsActive = false;
                newData.Status = (int)ProgramStatus.Pending;

                var response = _programRepository.CreateProgram(newData);

                await _context.SaveChangesAsync();

                if (response == null)
                    return ApiResponse<ProgramDto>.Failure(StatusCodes.Status400BadRequest, "Failed to create program");

                var resutlDto = _mapper.Map<ProgramDto>(newData);

                return ApiResponse<ProgramDto>.Success("Program created successfully", resutlDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ApiResponse<ProgramDto>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProgramDto>>> GetPrograms()
        {
            try
            {
                var query = _programRepository.GetPrograms();

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<ProgramDto>>(response);

                return ApiResponse<IEnumerable<ProgramDto>>.Success("Programs retrieved successfully", resultDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return ApiResponse<IEnumerable<ProgramDto>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred");
            }
        }

        public async Task<ApiResponse<IEnumerable<ProgramDto>>> GetProgram(string id)
        {
            try
            {
                var query = _programRepository.GetProgram(id);

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<ProgramDto>>(response);

                return ApiResponse<IEnumerable<ProgramDto>>.Success("Program retrieved successfully", resultDto);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return ApiResponse<IEnumerable<ProgramDto>>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> RemoveProgram(string dataId)
        {
            try
            {
                var data = _programRepository.GetProgram(dataId);

                if (data == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");

                var response = await _programRepository.RemoveProgram(dataId);

                if (!response)
                    return ApiResponse<bool>.Failure(StatusCodes.Status400BadRequest, "Failed to delete program");

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success("Program deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred");
            }
        }

        public async Task<ApiResponse<bool>> UpdateProgram(UpdateProgramDTO data)
        {
            try
            {
                var existingData = _programRepository.GetProgram(data.Id);

                if (existingData == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");

                if (data.StartDate < DateTime.Today)
                    return ApiResponse<bool>.Failure(StatusCodes.Status403Forbidden, "You cannot set Start date to a date in the past");

                _mapper.Map(data, existingData);

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success("Program updated successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }

    }
}
