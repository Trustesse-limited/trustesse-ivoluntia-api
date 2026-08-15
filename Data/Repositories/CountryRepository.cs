using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories;

public class CountryRepository : GenericRepository<Country>,ICountryRepository
{
    private readonly iVoluntiaDataContext _context;
    public CountryRepository(iVoluntiaDataContext context): base(context)
    {
            _context  = context;
    }
}