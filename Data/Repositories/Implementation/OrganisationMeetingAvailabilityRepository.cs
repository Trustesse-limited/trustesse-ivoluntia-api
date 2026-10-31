using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.IRepositories;
using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Implementation
{
    public class OrganisationMeetingAvailabilityRepository : GenericRepository<OrganisationMeetingAvailability>, IOrganisationMeetingAvailabilityRepository
    {
        public OrganisationMeetingAvailabilityRepository(iVoluntiaDataContext iVoluntiaDataContext) : base(iVoluntiaDataContext)
        {
        }
    }
}
