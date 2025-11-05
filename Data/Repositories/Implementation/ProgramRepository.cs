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
        public async Task<ApiResponse<string>> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                var program = await _context.Programs.Include(x => x.ProgramRejectionReasons).FirstOrDefaultAsync(x => x.Id == updateProgramStatusDto.ProgramId);
                if (program == null)
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, "program not found");
                }
                if (program.Status == 0 & updateProgramStatusDto.Status == "Pending" || program.Status == 1 & updateProgramStatusDto.Status == "Active" || program.Status == 8 & updateProgramStatusDto.Status == "Queried" || program.Status == 9 & updateProgramStatusDto.Status == "Ended")
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Cannot set new status to current status");
                }
                if (updateProgramStatusDto.Status != "Pending" & updateProgramStatusDto.Status != "Active" & updateProgramStatusDto.Status != "Queried" & updateProgramStatusDto.Status != "Ended")
                {
                    return ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "invalid status");
                }
                var superAdmin = program.ProgramRejectionReasons.FirstOrDefault(x => x.ProgramId == program.Id);
                if (updateProgramStatusDto.Status == "Active")
                {
                    program.Status = (int)ProgramStatus.Active;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", program.CreatedBy);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = _notificationRepository.ComposeNotificationAsync("programstatusupdate", "Email", placeHolder);
                    //notification service method(notification, program.CreatedBy(email of program creator)) to send email to creator(foundation admin)
                }
                else if (updateProgramStatusDto.Status == "Pending")
                {
                    program.Status = (int)ProgramStatus.Pending;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", superAdmin.QueriedByFullName);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = _notificationRepository.ComposeNotificationAsync("programstatusupdate", "email", placeHolder);
                    //notification service method(notification, program.QueriedByFullName(email of superadmin)) to send email to (superadmin)
                }
                else if (updateProgramStatusDto.Status == "Queried")
                {
                    program.Status = (int)ProgramStatus.Queried;
                    _context.Programs.Update(program);
                    var rejectionReason = new ProgramRejectionReason
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProgramId = program.Id,
                        QueriedBy = "put login userid",
                        QueriedMessage = updateProgramStatusDto.QueriedComment,
                        QueriedByFullName = "put the login username(superadmin email)"
                    };
                    await _context.ProgramRejectionReasons.AddAsync(rejectionReason);
                    await _context.SaveChangesAsync();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("UserName", program.CreatedBy);
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    var notification = _notificationRepository.ComposeNotificationAsync("programstatusupdate", "email", placeHolder);
                    //notification service method(notification, program.CreatedBy(email of program creator)) to send email to creator(foundation admin)
                }
                else
                {
                    program.Status = (int)ProgramStatus.Ended;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    var users = program.Users.Where(x => x.ProgramId == program.Id).ToList();
                    List<string> emails = new List<string>();
                    Dictionary<string, string> placeHolder = new Dictionary<string, string>();
                    placeHolder.Add("Title", program.Title);
                    placeHolder.Add("Status", updateProgramStatusDto.Status);
                    foreach (var user in users)
                    {
                        emails.Add(user.Email);
                    }
                    var notification = _notificationRepository.ComposeNotificationAsync("programended", "email", placeHolder);
                    //notification service method(notification, emails) to send email to volunters 
                }
                return ApiResponse<string>.Success("program status updated", $"{updateProgramStatusDto.Status}");
            }
            catch(Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"an error occurred {ex.Message}");
            }
        }
    }
}
