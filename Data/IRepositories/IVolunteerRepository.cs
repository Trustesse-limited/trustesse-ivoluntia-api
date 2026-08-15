using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;


public interface IVolunteerRepository : IGenericRepository<User>
{
   IQueryable<User> GetVolunteers(string foundationId, bool? isActive = null);
}

