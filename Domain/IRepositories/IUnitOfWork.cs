namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IUnitOfWork
{
    IGenericRepository<TEntity> repository<TEntity>() where TEntity : class;
    Task<int> CompleteAsync();
    ICountryRepository countryRepo { get; }
    IStateRepository stateRepo { get; }
}