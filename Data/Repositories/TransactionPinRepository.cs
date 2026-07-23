using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Domain.Entities;
using Trustesse.Ivoluntia.Domain.IRepositories;

namespace Trustesse.Ivoluntia.Data.Repositories
{
    public class TransactionPinRepository : GenericRepository<TransactionPin>, ITransactionPinRepository
    {
        public TransactionPinRepository(iVoluntiaDataContext context) : base(context)
        {
        }
    }
}
