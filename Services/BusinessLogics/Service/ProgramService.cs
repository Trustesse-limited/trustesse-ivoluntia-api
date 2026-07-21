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
                // string loginUserEmail = _currentUserService.GetUserEmail();
                if (_currentUserService.GetUserEmail() == null)
                    return ResponseHelper.BuildResponse("user not log in", StatusCodes.Status400BadRequest, "not log in", false);
                var program = await _uow.programRepo.GetByExpressionAsync(x => x.Id == updateProgramStatusDto.ProgramId);
                if (program == null)
                    return ResponseHelper.BuildResponse("program not found", StatusCodes.Status404NotFound, "no program found", false);
                if (program.Status == (int)ProgramStatus.Pending & updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() || program.Status == (int)ProgramStatus.Active & updateProgramStatusDto.Status == ProgramStatus.Active.ToString() || program.Status == (int)ProgramStatus.Queried & updateProgramStatusDto.Status == ProgramStatus.Queried.ToString() || program.Status == (int)ProgramStatus.Ended & updateProgramStatusDto.Status == ProgramStatus.Ended.ToString())
                    return ResponseHelper.BuildResponse("cannot set new status current status", StatusCodes.Status400BadRequest, "cannot set new status to current status", false);
                if (updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() && program.Status != (int)ProgramStatus.Queried)
                    return ResponseHelper.BuildResponse("cannot change status of program", StatusCodes.Status400BadRequest, "cannot change status of program", false);
                if (updateProgramStatusDto.Status == ProgramStatus.Active.ToString())
                {
                    program.Status = (int)ProgramStatus.Active;
                    _uow.programRepo.Update(program);
                    await _uow.CompleteAsync();

                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "Admin");
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = new List<string> { program.CreatedBy },
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ResponseHelper.BuildResponse("program status updated and email sent", StatusCodes.Status200OK, "program status updated", true);
                }
                else if (updateProgramStatusDto.Status == ProgramStatus.Pending.ToString())
                {
                    program.Status = (int)ProgramStatus.Pending;
                    _uow.programRepo.Update(program);
                    await _uow.CompleteAsync();
                    var programRejection = await _context.ProgramRejectionReasons.Where(x => x.ProgramId == program.Id).FirstOrDefaultAsync();

                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "Admin");
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = new List<string> { programRejection.CreatedBy },
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ResponseHelper.BuildResponse("program status updated and email sent to super admin", StatusCodes.Status200OK, "program status updated", true);
                }
                else if (updateProgramStatusDto.Status == ProgramStatus.Queried.ToString())
                {
                    program.Status = (int)ProgramStatus.Queried;
                    _uow.programRepo.Update(program);
                    var name = _currentUserService.GetUserFirstName();
                    var rejectionReason = new ProgramRejectionReason
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProgramId = program.Id,
                        QueriedBy = _currentUserService.GetUserEmail(),
                        QueriedMessage = updateProgramStatusDto.QueriedComment,
                        QueriedByFullName = name,
                        CreatedBy = _currentUserService.GetUserEmail()
                    };
                    await _uow.programRejectionReasonRepository.AddAsync(rejectionReason);
                    await _uow.CompleteAsync();

                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "Admin");
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = new List<string> { program.CreatedBy },
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ResponseHelper.BuildResponse("program status updated and email sent to admin", StatusCodes.Status200OK, "program status updated", true);
                }
                else
                {
                    program.Status = (int)ProgramStatus.Ended;
                    _uow.programRepo.Update(program);
                    await _uow.CompleteAsync();
                    var userProgram = await _uow.userProgramRepository.GetAsync(p => p.ProgramId == program.Id);

                    List<string> volunterEmails = new List<string>();
                    foreach (var item in userProgram)
                    {
                        volunterEmails.Add(item.CreatedBy);
                    }
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", "Admin");
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationService.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    EmailModel emailModel = new EmailModel
                    {
                        Receivers = volunterEmails,
                        Subject = "program status update",
                        Message = HttpUtility.HtmlDecode(notification.Data)
                    };
                    var emailResponse = await _emailService.SendEmailASync(emailModel);
                    return ResponseHelper.BuildResponse("program status updated and email sent to admin", StatusCodes.Status200OK, "program status updated", true);
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
            var userProgram = _uow.userProgramRepository.GetByExpression(up => up.ProgramId == programId && up.UserId == _currentUserService.GetUserId());
            var programGoal = await _uow.ProgramGoalRepository.GetByExpressionIncludeAsync(pg => pg.ProgramId == programId, pg => pg.Program);
            if (userProgram != null)
                return ResponseHelper.BuildResponse<string>("user in this program", StatusCodes.Status400BadRequest, null, false);
            if (programGoal.Program == null)
                return ResponseHelper.BuildResponse<string>("program not found", StatusCodes.Status400BadRequest, null, false);
            if (programGoal.Program.EndDate < DateTime.Now || programGoal.IsAchieved == true)
                return ResponseHelper.BuildResponse<string>("this program has ended", StatusCodes.Status400BadRequest, null, false);
            var addUserProgram = new UserProgram
            {
                ProgramId = programId,
                UserId = _currentUserService.GetUserId(),
                CreatedBy = _currentUserService.GetUserEmail(),
                DateCreated = DateTime.Now,
                Status = UserProgramStatusEnum.Pending.ToString()
            };
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
                Receivers = programGoal.Program.CreatedBy.Trim().Split().ToList(),
                Subject = "request to join program",
                Message = HttpUtility.HtmlDecode(adminNotificationCompose.Data)
            };
            var adminEmailResponse = await _emailService.SendEmailASync(adminEmailModel);
            return ResponseHelper.BuildResponse("email sent to volunteer and program admin", StatusCodes.Status200OK, "join program notification", true);
        }
        public async Task<GlobalRequestReponse<string>> LeaveProgram(string programId)
        {
            var userProgram = await _uow.userProgramRepository.GetByExpressionIncludeAsync(up => up.ProgramId == programId && up.UserId == _currentUserService.GetUserId(), up => up.Program);
            if (userProgram == null)
                return ResponseHelper.BuildResponse<string>("user not in program", StatusCodes.Status404NotFound, null, false);
            userProgram.Status = UserProgramStatusEnum.Left.ToString();
            _uow.userProgramRepository.Update(userProgram);
            await _uow.CompleteAsync();
            //send email to volunteer
            var userEmail = _currentUserService.GetUserEmail();
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
                Receivers = userProgram.Program.CreatedBy.Trim().Split().ToList(),
                Subject = "request to leave program",
                Message = HttpUtility.HtmlDecode(adminNotificationCompose.Data)
            };
            var adminEmailResponse = await _emailService.SendEmailASync(adminEmailModel);
            return ResponseHelper.BuildResponse("email sent to volunteer and admin", StatusCodes.Status200OK, "leave program notification", true);
        }
    }
}
