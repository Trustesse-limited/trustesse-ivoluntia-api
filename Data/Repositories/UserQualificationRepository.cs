using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UserQualificationRepository : GenericRepository<UserQualification>, IUserQualificationRepository
{
    public UserQualificationRepository(iVoluntiaDataContext context) : base(context)
    {
    }
}
