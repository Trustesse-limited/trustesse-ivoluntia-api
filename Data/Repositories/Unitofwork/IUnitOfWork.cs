using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> repository<TEntity>() where TEntity : class;
    Task<int> CompleteAsync();
    ICountryRepository countryRepo { get; }
    IStateRepository stateRepo { get; }
    IUserRepository userRepo { get; }
    IOnboardingProgressRepository  onboardingProgressRepo { get; }
    ILocationRepository locationRepo { get; }
}