using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Web;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Commons.Models.Response;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;
using Trustesse.Ivoluntia.Domain.IRepositories;
using Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class ProgramService : IProgramService
    {
        private readonly ILogger<ProgramService> _logger;
        private readonly iVoluntiaDataContext _context;
        private readonly IEmailService _emailService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly IFileUploadService _fileUploadService;
        private readonly IUnitOfWork _uow;
        public ProgramService(
            ILogger<ProgramService> logger,
            iVoluntiaDataContext context,
            IProgramRepository programRepository,
            IFoundationRepository foundationRepository,
            IEmailService emailService,
            ICurrentUserService currentUserService,
            IMapper mapper,
            IFileUploadService fileUploadService,
            INotificationService notificationService,
            IUnitOfWork uow)
        {
            _logger = logger;
            _context = context;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _emailService = emailService;
            _fileUploadService = fileUploadService;
            _notificationService = notificationService;
            _uow = uow;
        }
        public async Task<GlobalRequestReponse<ProgramDto>> CreateProgram(CreateProgramDto data)
        {
            try
            {

                var foundation = await _uow.foundationRepo.GetByExpressionAsync(f => f.Id == data.FoundationId);

                if (foundation == null)
                    return ResponseHelper.BuildResponse<ProgramDto>("Foundation not found", StatusCodes.Status404NotFound, null, false);

                if (!foundation.IsActive)
                    return ResponseHelper.BuildResponse<ProgramDto>("You cannot create a program for an inactive foundation", StatusCodes.Status403Forbidden, null, false);

                var programWithSameTitle = await _uow.programRepo.GetByExpressionAsync(p => p.Title.ToLower() == data.Title.ToLower());

                if (programWithSameTitle != null)
                    return ResponseHelper.BuildResponse<ProgramDto>("A program with the same title already exists", StatusCodes.Status409Conflict, null, false);

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

                await _uow.programRepo.AddAsync(newData);

                await _uow.CompleteAsync();

                var resutlDto = _mapper.Map<ProgramDto>(newData);

                return ResponseHelper.BuildResponse("Program created successfully", StatusCodes.Status200OK, resutlDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseHelper.BuildResponse<ProgramDto>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
        public async Task<GlobalRequestReponse<IEnumerable<ProgramDto>>> GetPrograms()
        {
            try
            {
                var query = _uow.programRepo.GetQueryable();

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<ProgramDto>>(response);

                return ResponseHelper.BuildResponse("Programs retrieved successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return ResponseHelper.BuildResponse<IEnumerable<ProgramDto>>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
        public async Task<GlobalRequestReponse<IEnumerable<ProgramDto>>> GetProgram(string id)
        {
            try
            {
                var query = _uow.programRepo.GetQueryable().Where(p => p.Id == id)
                     .Include(p => p.ProgramGoals)
                     .Include(p => p.ProgramSkills)
                        .ThenInclude(ps => ps.Skill);

                var response = await query.ToListAsync();

                var resultDto = _mapper.Map<IEnumerable<ProgramDto>>(response);

                return ResponseHelper.BuildResponse("Program retrieved successfully", StatusCodes.Status200OK, resultDto, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return ResponseHelper.BuildResponse<IEnumerable<ProgramDto>>("An error occurred", StatusCodes.Status500InternalServerError, null, false);
            }
        }
        public async Task<GlobalRequestReponse<bool>> RemoveProgram(string dataId)
        {
            try
            {
                var data = await _uow.programRepo.GetByExpressionAsync(p => p.Id == dataId);

                if (data == null)
                    return ResponseHelper.BuildResponse("Program not found", StatusCodes.Status404NotFound, false, false);

                await _uow.programRepo.DeleteAsync(data);

                await _uow.CompleteAsync();

                return ResponseHelper.BuildResponse("Program deleted successfully", StatusCodes.Status200OK, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseHelper.BuildResponse("An error occurred", StatusCodes.Status500InternalServerError, false, false);
            }
        }
        public async Task<GlobalRequestReponse<bool>> UpdateProgram(UpdateProgramDTO data)
        {
            try
            {
                var existingData = await _uow.programRepo.GetByExpressionAsync(p => p.Id == data.Id);

                if (existingData == null)
                    return ResponseHelper.BuildResponse("Program not found", StatusCodes.Status404NotFound, false, false);

                if (data.StartDate < DateTime.Today)
                    return ResponseHelper.BuildResponse("You cannot set Start date to a date in the past", StatusCodes.Status403Forbidden, false, false);

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

                return ResponseHelper.BuildResponse("Program updated successfully", StatusCodes.Status200OK, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseHelper.BuildResponse("An error occurred", StatusCodes.Status500InternalServerError, false, false);
            }
        }
        public async Task<GlobalRequestReponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                var response = await _uow.programRepo.UpdateProgramStatusAsync(updateProgramStatusDto);
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
                    return ResponseHelper.BuildResponse("program status updated and email sent to", StatusCodes.Status200OK, $"{responsesplit[0]}", true);
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
                    return ResponseHelper.BuildResponse("program status updated and email sent to volunteers", StatusCodes.Status200OK, $"{responsesplit[0]}", true);
                }
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status400BadRequest, null, false);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<string>(ex.Message, StatusCodes.Status500InternalServerError, null, false);
            }
        }
        public async Task<GlobalRequestReponse<bool>> DeleteProgramGoals(string programGoalId)
        {
            try
            {
                var userId = _currentUserService.GetUserId();

                if (userId == null)
                    return ResponseHelper.BuildResponse("You must log in first", StatusCodes.Status401Unauthorized, false, false);

                var userFoundationId = _currentUserService.GetUserFoundationId();

                var goal = await _context.ProgramGoals.Include(g => g.Program).FirstOrDefaultAsync(g => g.Id == programGoalId);

                if (goal == null)
                    return ResponseHelper.BuildResponse("Program Goal not found", StatusCodes.Status404NotFound, false, false);

                if (goal.Program.FoundationId != userFoundationId)
                    return ResponseHelper.BuildResponse("You are not allowed to delete this program goal", StatusCodes.Status403Forbidden, false, false);

                if (goal.Program.HasProgramEnded())
                    return ResponseHelper.BuildResponse("Program already ended", StatusCodes.Status403Forbidden, false, false);

                if (goal.IsAchieved)
                    return ResponseHelper.BuildResponse("You are not allowed to delete achieved goal", StatusCodes.Status403Forbidden, false, false);

                _context.ProgramGoals.Remove(goal);

                await _context.SaveChangesAsync();

                return ResponseHelper.BuildResponse("Program Goal deleted successfully", StatusCodes.Status200OK, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return ResponseHelper.BuildResponse("An error occurred", StatusCodes.Status500InternalServerError, false, false);
            }
        }
        public async Task<GlobalRequestReponse<string>> JoinProgram(string programId)
        {
            var response = await _uow.programRepo.JoinProgram(programId, _currentUserService.GetUserId());
            if (response == "user already in this program")
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status400BadRequest, null, false);
            if (response == "this program has ended")
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status400BadRequest, null, false);
            if (response == "program not found")
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status404NotFound, null, false);

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
            return ResponseHelper.BuildResponse("email sent to volunteer and program admin", StatusCodes.Status200OK, "join program notification", true);
        }
        public async Task<GlobalRequestReponse<string>> LeaveProgram(string programId)
        {
            var response = await _uow.programRepo.LeaveProgram(programId, _currentUserService.GetUserId());
            if (response == "program not found")
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status404NotFound, null, false);
            if (response == "user not found")
                return ResponseHelper.BuildResponse<string>(response, StatusCodes.Status404NotFound, null, false);
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
            return ResponseHelper.BuildResponse("email sent to volunteer and admin", StatusCodes.Status200OK, "leave program notification", true);
        }
    }
}
