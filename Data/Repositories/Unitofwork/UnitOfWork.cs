using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public readonly iVoluntiaDataContext _dbContext;
    private readonly ICurrentUserRepository _currentUserRepository;
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
    public IFavoriteProgramRepository favoriteProgramRepo { get; set; }
    public IFoundationRepository foundationRepo { get; set; }
    public IProgramRepository programRepo { get; set; }
    public ISecurityQuestionRepository securityQuestionRepo { get; set; }
    public IUserSecurityQuestionRepository userSecurityQuestionRepo { get; set; }
    public IUserSecurityValidationAttemptRepository userSecurityValidationAttemptRepo { get; set; }
    public IOtpRepository otpRepo { get; set; }
    public IVolunteerRepository volunteerRepo { get; set; }



    public UnitOfWork(iVoluntiaDataContext dbContext, ICurrentUserRepository currentUserRepository)
    {
        _dbContext = dbContext;
        _currentUserRepository = currentUserRepository;
        countryRepo = new CountryRepository(dbContext);
        stateRepo = new StateRepository(dbContext);
        userRepo = new UserRepository(dbContext);
        locationRepo = new LocationRepository(dbContext);
        userInterestLinkRepo = new UserInterestLinkRepository(dbContext);
        userSkillLinkRepo = new UserSkillLinkRepository(dbContext);
        interestRepo = new InterestRepository(dbContext);
        skillRepo = new SkillRepository(dbContext);
        onboardingProgressRepo = new OnboardingProgressRepository(dbContext);
        refreshTokenRepo = new RefreshTokenRepository(dbContext);
        favoriteProgramRepo = new FavoriteProgramRepository(dbContext);
        foundationRepo = new FoundationRepository(dbContext);
        programRepo = new ProgramRepository(dbContext, currentUserRepository);
        securityQuestionRepo = new SecurityQuestionRepository(dbContext);
        userSecurityQuestionRepo = new UserSecurityQuestionRepository(dbContext);
        userSecurityValidationAttemptRepo = new UserSecurityValidationAttemptRepository(dbContext);
        otpRepo = new OtpRepository(dbContext);
        volunteerRepo = new VolunteerRepository(dbContext);
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