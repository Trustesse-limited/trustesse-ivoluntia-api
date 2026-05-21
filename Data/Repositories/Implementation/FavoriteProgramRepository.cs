using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class FavoriteProgramRepository
    {
        private readonly iVoluntiaDataContext _context;
        private readonly INotificationRepository _notificationRepository;
        private readonly RoleManager<Role> _roleManager;
        private readonly UserManager<User> _userManager;
        private readonly ICurrentUserRepository _currentUserRepository;

        public FavoriteProgramRepository(iVoluntiaDataContext context, INotificationRepository notificationRepository, RoleManager<Role> roleManager, UserManager<User> userManager, ICurrentUserRepository currentUserRepository)
        {
            _context = context;
            _notificationRepository = notificationRepository;
            _roleManager = roleManager;
            _userManager = userManager;
            _currentUserRepository = currentUserRepository;
        }

        public async Task<FavoriteProgram> AddFavoriteProgram(FavoriteProgram data)
        {
            await _context.FavoritePrograms.AddAsync(data);
            return data;
        }

        public IQueryable<FavoriteProgram> GetFavoritePrograms()
        {
            return _context.FavoritePrograms.AsQueryable();
        }

        public async Task<bool> RemoveFavoriteProgram(string dataId)
        {
            var data = await _context.FavoritePrograms.Where(p => p.Id == dataId).FirstAsync();

            _context.FavoritePrograms.Remove(data);

            return true;
        }
    }
}



