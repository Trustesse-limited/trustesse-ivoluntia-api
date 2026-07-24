using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Domain.IRepositories
{
    public interface IVolunteerRepository : IGenericRepository<User>
    {
        IQueryable<User> GetVolunteers(string foundationId, bool? isActive = null);
    }
}
