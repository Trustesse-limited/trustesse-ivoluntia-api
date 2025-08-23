using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    private readonly iVoluntiaDataContext _context;

    public UserRepository(iVoluntiaDataContext context) : base(context)
    {
            _context = context;
    }
    
}

public class OnboardingProgressRepository : GenericRepository<OnboardingProgress>, IOnboardingProgressRepository
{
    private readonly iVoluntiaDataContext _context;

    public OnboardingProgressRepository(iVoluntiaDataContext context) : base(context)
    {
        _context = context;
    }
}