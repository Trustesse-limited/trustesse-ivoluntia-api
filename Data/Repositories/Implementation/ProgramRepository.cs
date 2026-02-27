using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.Program;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Migrations;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.Enums;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class ProgramRepository : IProgramRepository
    {
        private readonly iVoluntiaDataContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly RoleManager<User> _roleManager;

        public ProgramRepository(RoleManager<User> roleManager)
        {
            _roleManager = roleManager;
        }

        public ProgramRepository(iVoluntiaDataContext context, INotificationRepository notificationRepository, IHttpContextAccessor contextAccessor)
        {
            _context = context;
            _notificationRepository = notificationRepository;
        }
        public async Task<Program> CreateProgram(Program data)
        {
            await _context.Programs.AddAsync(data);
            return data;
        }


        public IQueryable<Program> GetProgram(string dataId)
        {
            var query = _context.Programs
                .Include(p => p.ProgramGoals)
                .Include(p => p.ProgramSkills)
                    .ThenInclude(ps => ps.Skill)
                .AsQueryable();

            if (!string.IsNullOrEmpty(dataId))
                query = query.Where(p => p.Id == dataId);

            return query;
        }


        public IQueryable<Program> GetPrograms()
        {
            return _context.Programs.AsQueryable();
        }


        public async Task<bool> RemoveProgram(string dataId)
        {
            var data = await _context.Programs.Where(p => p.Id == dataId).FirstAsync();

            _context.Programs.Remove(data);

            return true;

        }

        public bool UpdateProgram(Program data)
        {
            _context.Programs.Update(data);

            return true;
        }
        public async Task<ApiResponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto, string id)
        {
            try
            {
                if(id == null)
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "user not log in");
                }
                var program = await _context.Programs.Include(x => x.Users).Include(x => x.ProgramRejectionReasons).FirstOrDefaultAsync(x => x.Id == updateProgramStatusDto.ProgramId);
                var foundationAdminEmail = program.Users.Where(x => x.ProgramId == program.Id && x.Email == program.CreatedBy).FirstOrDefault();
                if (program == null)
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, "program not found");
                }
                if (program.Status == (int)ProgramStatus.Pending & updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() || program.Status == (int)ProgramStatus.Active & updateProgramStatusDto.Status == ProgramStatus.Active.ToString() || program.Status == (int)ProgramStatus.Queried & updateProgramStatusDto.Status == ProgramStatus.Queried.ToString() || program.Status == (int)ProgramStatus.Ended & updateProgramStatusDto.Status == ProgramStatus.Ended.ToString())
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Cannot set new status to current status");
                }
                if (updateProgramStatusDto.Status != ProgramStatus.Pending.ToString() & updateProgramStatusDto.Status != ProgramStatus.Active.ToString() & updateProgramStatusDto.Status != ProgramStatus.Queried.ToString() & updateProgramStatusDto.Status != ProgramStatus.Ended.ToString())
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "invalid status");
                }
                if(updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() && program.Status != (int)ProgramStatus.Queried)
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "cannot change status of program");
                }
                if (updateProgramStatusDto.Status == ProgramStatus.Active.ToString())
                {
                    program.Status = (int)ProgramStatus.Active;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", program.CreatedBy);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationRepository.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    return ApiResponse<string>.Success($"{foundationAdminEmail}", notification.Data);
                }
                else if (updateProgramStatusDto.Status == ProgramStatus.Pending.ToString())
                {
                    program.Status = (int)ProgramStatus.Pending;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", program.CreatedBy);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationRepository.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    return ApiResponse<string>.Success($"{foundationAdminEmail}", notification.Data);
                }   
                else if (updateProgramStatusDto.Status == ProgramStatus.Queried.ToString())
                {
                    var admin = await _context.Users.Where(x => x.Id == id).FirstOrDefaultAsync();   
                    program.Status = (int)ProgramStatus.Queried;
                    _context.Programs.Update(program);
                    var rejectionReason = new ProgramRejectionReason
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProgramId = program.Id,
                        QueriedBy = admin.Id,
                        QueriedMessage = updateProgramStatusDto.QueriedComment,
                        QueriedByFullName = admin.Email
                    };
                    await _context.ProgramRejectionReasons.AddAsync(rejectionReason);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", program.CreatedBy);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = await _notificationRepository.ComposeNotificationAsync(NotificationTypeEnum.ProgramStatusUpdate.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    return ApiResponse<string>.Success($"{foundationAdminEmail} {admin.Email}", notification.Data);
                }
                else
                {
                    program.Status = (int)ProgramStatus.Ended;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    var users = program.Users.Where(x => x.ProgramId == program.Id).ToList();

                    string emails = "";
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    foreach (var user in users)
                    {
                        emails = emails + user.Email + ' ';
                    }
                    var notification = await _notificationRepository.ComposeNotificationAsync(NotificationTypeEnum.ProgramEnded.ToString(), NotificationChannelEnum.Email.ToString(), placeHolder);
                    return ApiResponse<string>.Success(emails, notification.Data);
                }
            }
            catch(Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"an error occurred {ex.Message}");
            }
        }
    }
}
