using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class LocationRepository : GenericRepository<Location>, ILocationRepository
{
    private readonly iVoluntiaDataContext _context;
    public LocationRepository(iVoluntiaDataContext  context) : base(context)
    {
        _context = context;
    }
}