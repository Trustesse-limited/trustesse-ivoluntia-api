using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
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
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserRepository _currentUserRepository;
        public ProgramRepository(iVoluntiaDataContext context, INotificationRepository notificationRepository, RoleManager<Role> roleManager, UserManager<User> userManager, ICurrentUserRepository currentUserRepository)
        {
            _context = context;
            _notificationRepository = notificationRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _currentUserRepository = currentUserRepository;
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
        public async Task<string> UpdateProgramStatusAsync(UpdateProgramStatusDto updateProgramStatusDto)
        {
            try
            {
                string loginUserEmail = _currentUserRepository.GetUserEmail();
                if(loginUserEmail == null)
                {
                    return ("user not log in");
                }
                var program = await _context.Programs.FirstOrDefaultAsync(x => x.Id == updateProgramStatusDto.ProgramId);
                if (program == null)
                {
                    return ("program not found");
                }
                if (program.Status == (int)ProgramStatus.Pending & updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() || program.Status == (int)ProgramStatus.Active & updateProgramStatusDto.Status == ProgramStatus.Active.ToString() || program.Status == (int)ProgramStatus.Queried & updateProgramStatusDto.Status == ProgramStatus.Queried.ToString() || program.Status == (int)ProgramStatus.Ended & updateProgramStatusDto.Status == ProgramStatus.Ended.ToString())
                {
                    return ("Cannot set new status to current status");
                }
                if (updateProgramStatusDto.Status != ProgramStatus.Pending.ToString() & updateProgramStatusDto.Status != ProgramStatus.Active.ToString() & updateProgramStatusDto.Status != ProgramStatus.Queried.ToString() & updateProgramStatusDto.Status != ProgramStatus.Ended.ToString())
                {
                    return ("invalid status");
                }
                if(updateProgramStatusDto.Status == ProgramStatus.Pending.ToString() && program.Status != (int)ProgramStatus.Queried)
                {
                    return ("cannot change status of program");
                }
                if (updateProgramStatusDto.Status == ProgramStatus.Active.ToString())
                {
                    program.Status = (int)ProgramStatus.Active;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    return ($"foundationAdmin&{program.CreatedBy}&{program.Title}");
                }
                else if (updateProgramStatusDto.Status == ProgramStatus.Pending.ToString())
                {
                    program.Status = (int)ProgramStatus.Pending;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    var programRejection = await _context.ProgramRejectionReasons.Where(x => x.ProgramId == program.Id).FirstOrDefaultAsync();
                    return ($"superAdmin&{programRejection.CreatedBy}&{program.Title}");
                }
                else if (updateProgramStatusDto.Status == ProgramStatus.Queried.ToString())
                {  
                    program.Status = (int)ProgramStatus.Queried;
                    _context.Programs.Update(program);
                    var rejectionReason = new ProgramRejectionReason
                    {
                        Id = Guid.NewGuid().ToString(),
                        ProgramId = program.Id,
                        QueriedBy = loginUserEmail,
                        QueriedMessage = updateProgramStatusDto.QueriedComment,
                        QueriedByFullName = loginUserEmail.Split('@')[0]
                    };
                    await _context.ProgramRejectionReasons.AddAsync(rejectionReason);
                    await _context.SaveChangesAsync();
                    return ($"foundationAdmin&{program.CreatedBy}&{program.Title}");
                }
                else
                {
                    program.Status = (int)ProgramStatus.Ended;
                    _context.Programs.Update(program);
                    await _context.SaveChangesAsync();
                    var userProgram = await _context.userPrograms.Where(p => p.ProgramId == program.Id).ToListAsync();

                    string emails = "";
                    foreach (var item in userProgram)
                    {
                        emails = emails + item.CreatedBy + ' ';
                    }
                    return ($"volunteers&{emails}&{program.Title}");
                }
            }
            catch(Exception ex)
            {
                return ($"an error occurred {ex.Message}");
            }
        }
        public async Task<string> JoinProgram(string programId, string userId)
        {
            var userProgram = await _context.userPrograms.Where(x => x.UserId == userId && x.ProgramId == programId).FirstOrDefaultAsync();
            var programGoal = await _context.ProgramGoals.Include(x => x.Program).Where(x => x.ProgramId == programId).FirstOrDefaultAsync();
            if (userProgram != null)
            {
                return "user already in this program";
            }
            if (programGoal.Program.EndDate < DateTime.Now || programGoal.IsAchieved == true)
            {
                return "this program has ended";
            }
            var addUserProgram = new UserProgram
            {
                ProgramId = programId,  
                UserId = userId,    
                CreatedBy = _currentUserRepository.GetUserEmail(),
                DateCreated = DateTime.Now, 
                Status = UserProgramStatusEnum.Pending.ToString()
            };
            await _context.userPrograms.AddAsync(addUserProgram); 
            await _context.SaveChangesAsync();
            return programGoal.Program.CreatedBy;
        }
        public async Task<string> LeaveProgram(string programId,string userId)
        {
            var userProgram = await _context.userPrograms.Include(p => p.Program).Where(x => x.UserId == userId && x.ProgramId == programId).FirstOrDefaultAsync();
            if(userProgram == null)
            {
                return "user not found";
            }
            userProgram.Status = UserProgramStatusEnum.Left.ToString();     
            _context.userPrograms.Update(userProgram);  
            await _context.SaveChangesAsync();
            return userProgram.Program.CreatedBy;
        }
    }
}
