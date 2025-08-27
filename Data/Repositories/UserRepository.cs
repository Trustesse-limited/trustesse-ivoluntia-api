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

public class UserInterestLinkRepository : GenericRepository<UserInterestLink>, IUserInterestLinkRepository
{
    private readonly iVoluntiaDataContext _context;

    public UserInterestLinkRepository(iVoluntiaDataContext  context) : base(context)
    {
        _context = context;
    }
}

public class UserSkillLinkRepository : GenericRepository<UserSkillLink>, IUserSkillLinkRepository
{
    private readonly iVoluntiaDataContext _context;

    public UserSkillLinkRepository(iVoluntiaDataContext  context) : base(context)
    {
        _context = context;
    }
}

public class SkillRepository : GenericRepository<Skill>, ISkillRepository
{
    private readonly iVoluntiaDataContext _context;

    public SkillRepository(iVoluntiaDataContext  context) : base(context)
    {
        _context = context;
    }
}

public class InterestRepository : GenericRepository<Interest>, IInterestRepository
{
    private readonly iVoluntiaDataContext _context;

    public InterestRepository(iVoluntiaDataContext  context) : base(context)
    {
        _context = context;
    }
}

