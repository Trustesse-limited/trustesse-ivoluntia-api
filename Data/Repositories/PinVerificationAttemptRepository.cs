using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class PinVerificationAttemptRepository : GenericRepository<PinVerificationAttempt>, IPinVerificationAttemptRepository
    {
        public PinVerificationAttemptRepository(iVoluntiaDataContext context) : base(context)
        {
        }
    }
}
