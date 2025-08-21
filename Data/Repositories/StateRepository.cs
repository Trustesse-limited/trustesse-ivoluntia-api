using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class StateRepository : GenericRepository<State>, IStateRepository
{
    private readonly iVoluntiaDataContext _context;
    public StateRepository(iVoluntiaDataContext context) : base(context)
    {
            _context = context;
    }
} 