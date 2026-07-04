namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> repository<TEntity>() where TEntity : class;
    Task<int> CompleteAsync();
    ICountryRepository countryRepo { get; }
    IStateRepository stateRepo { get; }
    IUserRepository userRepo { get; }
    IOnboardingProgressRepository onboardingProgressRepo { get; }
    ILocationRepository locationRepo { get; }
    IUserInterestLinkRepository userInterestLinkRepo { get; }
    IUserSkillLinkRepository userSkillLinkRepo { get; }
    IInterestRepository interestRepo { get; }
    ISkillRepository skillRepo { get; }
    IRefreshTokenRepository refreshTokenRepo { get; set; }
    IOrganizationRepository OrganizationRepository { get; set; }
    ICauseFoundationRepository CauseFoundationRepository {  get; set; }
    ICauseRepository CauseRepository { get; set; }
    ICategoryRepository CategoryRepository { get; set; }
    IOtpRepo OtpRepo { get; set; }  
}