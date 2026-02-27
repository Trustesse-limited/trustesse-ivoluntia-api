using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Implementations
{
    public class ProgramService : IProgramService
    {
        private readonly ILogger<ProgramService> _logger;
        private readonly iVoluntiaDataContext _context;
        private readonly IProgramRepository _programRepository;
        private readonly IFoundationRepository _foundationRepository;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        public ProgramService(
            ILogger<ProgramService> logger,
            iVoluntiaDataContext context,
            IProgramRepository programRepository,
            IFoundationRepository foundationRepository,
            IEmailService emailService, 
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _logger = logger;
            _context = context;
            _programRepository = programRepository;
            _foundationRepository = foundationRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _emailService = emailService;
        }
        public async Task<ApiResponse<ProgramDto>> CreateProgram(CreateProgramDto data)
        {
            try
            {
                var foundation = _foundationRepository.GetFoundation(data.FoundationId).FirstOrDefault();

                if (foundation == null)
                    return ApiResponse<ProgramDto>.Failure(StatusCodes.Status404NotFound, "Foundation not found");

                if (!foundation.IsActive)
                    return ApiResponse<ProgramDto>.Failure(StatusCodes.Status403Forbidden, "You cannot create a program for an inactive foundation");

                var programWithSameTitle = _programRepository.GetPrograms().FirstOrDefault(p => p.Title.ToLower() == data.Title.ToLower());

                if (programWithSameTitle != null)
                    return ApiResponse<ProgramDto>.Failure(StatusCodes.Status409Conflict, "A program with the same title already exists");


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
                newData.CreatedBy = data.CreatorEmail;
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
        public async Task<ApiResponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                var id = _currentUserService.GetUserId();
                var programStatus = await _programRepository.UpdateProgramStatusAsync(updateProgramStatusDto, id);
                if(programStatus.StatusCode != StatusCodes.Status200OK)
                {
                    return ApiResponse<string>.Failure(programStatus.StatusCode, programStatus.Message);
                }
                EmailModel emailModel = new EmailModel
                {
                    Receivers = programStatus.Message.TrimEnd().Split(' ').ToList(),   
                    Subject = "program status update",
                    Message = HttpUtility.HtmlDecode(programStatus.Data)
                };
                var emailResponse = await _emailService.SendEmailASync(emailModel);
                if (programStatus.StatusCode == StatusCodes.Status200OK & emailResponse.StatusCode == StatusCodes.Status200OK)
                {
                    return ApiResponse<string>.Success($"program status updated and email sent to {programStatus.Message}", $"{programStatus.Data}");
                }
                if(programStatus.StatusCode == StatusCodes.Status200OK)
                {
                    return ApiResponse<string>.Success($"{programStatus.Message}", $"{programStatus.Data}");
                }
                return ApiResponse<string>.Failure(programStatus.StatusCode, $"{programStatus.Message}");
            }
            catch(Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, ex.Message);
            }
         }

        public async Task<ApiResponse<bool>> DeleteProgramGoals(string programGoalId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status401Unauthorized, "You must log in first");

                var userFoundationId = await _currentUserService.GetUserFoundationId(userId);

                var goal = await _context.ProgramGoals.Include(g => g.Program).FirstOrDefaultAsync(g => g.Id == programGoalId);

                if (goal == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program Goal not found");

                if (goal.Program.FoundationId != userFoundationId.Data)
                    return ApiResponse<bool>.Failure(StatusCodes.Status403Forbidden, "You are not allowed to delete this program goal");

                if (goal.Program.HasProgramEnded())
                    return ApiResponse<bool>.Failure(StatusCodes.Status403Forbidden, "Program already ended");

                if (goal.IsAchieved)
                    return ApiResponse<bool>.Failure(StatusCodes.Status403Forbidden, "You are not allowed to delete achieved goal");

                _context.ProgramGoals.Remove(goal);

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.Success("Program Goal deleted successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ApiResponse<bool>.Failure(StatusCodes.Status500InternalServerError, "An error occurred");
            }
        }
    }
}
