using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Domain.IRepositories;

public interface IStateRepository : IGenericRepository<State>
{
    Task<IEnumerable<State>> GetStateByCountryId(string countryId);
}