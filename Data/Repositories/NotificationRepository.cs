using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        private readonly iVoluntiaDataContext _context;
        public NotificationRepository(iVoluntiaDataContext context) : base(context)
        {
            _context = context;
        }
    }
}
