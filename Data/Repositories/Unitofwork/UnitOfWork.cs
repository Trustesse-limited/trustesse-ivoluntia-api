using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public readonly iVoluntiaDataContext _dbContext;
    private Hashtable _repositories;
    public DatabaseFacade Database => _dbContext.Database;

    public ICountryRepository countryRepo { get; set; }
    public IStateRepository stateRepo { get; set; }
    public IUserRepository userRepo { get; set; }
    public IOnboardingProgressRepository onboardingProgressRepo { get; set; }
    public ILocationRepository locationRepo { get; set; }
    public IUserInterestLinkRepository userInterestLinkRepo { get; set; }
    public IUserSkillLinkRepository userSkillLinkRepo { get; set; }
    public IInterestRepository interestRepo { get; set; }
    public ISkillRepository skillRepo { get; set; }
    public IRefreshTokenRepository refreshTokenRepo { get; set; }

    public UnitOfWork(iVoluntiaDataContext dbContext)
    {
        _dbContext = dbContext;
        countryRepo = new CountryRepository(dbContext);
        stateRepo = new StateRepository(dbContext);
        userRepo = new UserRepository(dbContext);
        locationRepo = new LocationRepository(dbContext);
        userInterestLinkRepo = new UserInterestLinkRepository(dbContext);
        userSkillLinkRepo = new UserSkillLinkRepository(dbContext);
        interestRepo = new InterestRepository(dbContext);
        skillRepo = new SkillRepository(dbContext);
        onboardingProgressRepo = new OnboardingProgressRepository(dbContext);
    }
    public IGenericRepository<TEntity> repository<TEntity>() where TEntity : class
    {
        if (_repositories == null) _repositories = new Hashtable();
        var Type = typeof(TEntity).Name;
        if (!_repositories.ContainsKey(Type))
        {
            var repositoryType = typeof(GenericRepository<TEntity>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _dbContext);
            _repositories.Add(Type, repositoryInstance);
        }
        return (IGenericRepository<TEntity>)_repositories[Type];
    }

    public async Task<int> CompleteAsync()
    {
        return await _dbContext.SaveChangesAsync();
    }

}