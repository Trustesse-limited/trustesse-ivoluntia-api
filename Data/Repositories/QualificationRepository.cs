using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class QualificationRepository : GenericRepository<Qualification>, IQualificationRepository
{
    public QualificationRepository(iVoluntiaDataContext context) : base(context)
    {
    }
}
