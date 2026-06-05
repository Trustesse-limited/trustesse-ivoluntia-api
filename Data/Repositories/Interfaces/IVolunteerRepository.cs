using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IVolunteerRepository
    {
        IQueryable<User> GetVolunteers(string foundationId, bool? isActive = null);
    }
}
