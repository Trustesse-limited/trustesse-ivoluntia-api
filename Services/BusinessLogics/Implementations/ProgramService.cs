using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
        private readonly INotificationService _notificationService;     
        private readonly IMapper _mapper;
        private readonly IFileUploadService _fileUploadService;
        public ProgramService(
            ILogger<ProgramService> logger,
            iVoluntiaDataContext context,
            IProgramRepository programRepository,
            IFoundationRepository foundationRepository,
            IEmailService emailService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IFileUploadService fileUploadService,
            INotificationService notificationService)
        {
            _logger = logger;
            _context = context;
            _programRepository = programRepository;
            _foundationRepository = foundationRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _emailService = emailService;
            _fileUploadService = fileUploadService;
            _notificationService = notificationService;
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


                if (!string.IsNullOrWhiteSpace(data.BannerImage))
                {
                    string fileName = Guid.NewGuid().ToString();
                    var imageUrl = await _fileUploadService.UploadImageFromBase64Async(data.BannerImage, fileName);
                    newData.BannerImage = imageUrl;
                }

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
                var existingData = await _programRepository.GetProgram(data.Id).FirstOrDefaultAsync();

                if (existingData == null)
                    return ApiResponse<bool>.Failure(StatusCodes.Status404NotFound, "Program not found");

                if (data.StartDate < DateTime.Today)
                    return ApiResponse<bool>.Failure(StatusCodes.Status403Forbidden, "You cannot set Start date to a date in the past");

                _mapper.Map(data, existingData);

                if (!string.IsNullOrWhiteSpace(data.BannerImage))
                {
                    if (data.BannerImage.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.Equals(existingData.BannerImage, data.BannerImage, StringComparison.OrdinalIgnoreCase))
                        {
                            existingData.BannerImage = data.BannerImage;
                        }
                    }
                    else
                    {
                        string fileName = Guid.NewGuid().ToString();

                        var imageUrl = await _fileUploadService.UploadImageFromBase64Async(data.BannerImage, fileName);

                        if (!string.IsNullOrWhiteSpace(imageUrl))
                        {
                            existingData.BannerImage = imageUrl;
                        }
                    }
                }

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
                var response = await _programRepository.UpdateProgramStatusAsync(updateProgramStatusDto);
                var responsesplit = response.Split('&');
                if (responsesplit[0] == "foundationAdmin" || responsesplit[0] == "superAdmin")
                {
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "Admin");
                    placeHolder.Add("Title", responsesplit[2]);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = responsesplit[1].TrimEnd().Split(' ').ToList(),
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ApiResponse<string>.Success($"program status updated and email sent to", $"{responsesplit[0]}");
                }
                if (responsesplit[0] == "volunteers")
                {
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "volunteer");
                    placeHolder.Add("Title", responsesplit[2]);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramEnded.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = responsesplit[1].TrimEnd().Split(' ').ToList(),
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ApiResponse<string>.Success($"program status updated and email sent to volunteers", $"{responsesplit[0]}");
                }
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, response);
            }
            catch (Exception ex)
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
        public async Task<ApiResponse<string>> JoinProgram(string programId)
        {
            var response = await _programRepository.JoinProgram(programId, _currentUserService.GetUserId());
            if(response == "user already in this program")
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, response);
            if(response == "this program has ended")
                return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, response);
            //send email to volunteer
            var userEmail = _currentUserService.GetUserEmail();
            string name = _currentUserService.GetUserFirstName();
            Dictionary<string, string> placeHolder = new Dictionary<string, string>();
            placeHolder.Add("Name", name);
            var notificationCompose = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.JoinProgram.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
            EmailModel volunteerEmailModel = new EmailModel
            {
                Receivers = userEmail.Trim().Split().ToList(),
                Subject = "request to join program",
                Message = HttpUtility.HtmlDecode(notificationCompose.Data)
            };
            var volunteerEmailResponse = await _emailService.SendEmailASync(volunteerEmailModel);
            //send email to program admin
            Dictionary<string, string> adminPlaceHolder = new Dictionary<string, string>();
            adminPlaceHolder.Add("Name", "Admin");
            adminPlaceHolder.Add("VolunteerEmail", userEmail);
            var adminNotificationCompose = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.RequestToJoinProgram.ToString(), NotificationChannelEnum.Email.ToString(), adminPlaceHolder);
            EmailModel adminEmailModel = new EmailModel
            {
                Receivers = response.Trim().Split().ToList(),
                Subject = "request to join program",
                Message = HttpUtility.HtmlDecode(adminNotificationCompose.Data)
            };
            var adminEmailResponse = await _emailService.SendEmailASync(adminEmailModel);
            return ApiResponse<string>.Success("email sent to volunteer and program admin", "join program notification");
        }
        public async Task<ApiResponse<string>> LeaveProgram(string programId)
        {
            var response = await _programRepository.LeaveProgram(programId, _currentUserService.GetUserId());
            if(response == "user not found")
                return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, response);
            var userEmail = _currentUserService.GetUserEmail();
            //send email to volunteer
            string name = _currentUserService.GetUserFirstName();   
            Dictionary<string, string> placeHolder = new Dictionary<string, string>();
            placeHolder.Add("Name", name);
            var notificationCompose = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.LeaveProgram.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
            EmailModel volunteerEmailModel = new EmailModel
            {
                Receivers = userEmail.Trim().Split().ToList(),
                Subject = "request to leave program",
                Message = HttpUtility.HtmlDecode(notificationCompose.Data)
            };
            var volunteerEmailResponse = await _emailService.SendEmailASync(volunteerEmailModel);
            //send email to program admin
            Dictionary<string, string> adminPlaceHolder = new Dictionary<string, string>();
            adminPlaceHolder.Add("Name", "Admin");
            adminPlaceHolder.Add("volunteerEmail", userEmail);
            var adminNotificationCompose = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.RequestToLeaveProgram.ToString(), NotificationChannelEnum.Email.ToString(), adminPlaceHolder);
            EmailModel adminEmailModel = new EmailModel
            {
                Receivers = response.Trim().Split().ToList(),
                Subject = "request to leave program",
                Message = HttpUtility.HtmlDecode(adminNotificationCompose.Data)
            };
            var adminEmailResponse = await _emailService.SendEmailASync(adminEmailModel);
            return ApiResponse<string>.Success("email sent to volunteer and admin", "leave program notification");
        }
    }
}
