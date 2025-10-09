using Trustesse.Ivoluntia.Domain.Entities;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface IFoundationRepository
    {
        Task<Foundation> CreateFoundation(Foundation data);
        Task<bool> RemoveFoundation(string dataId);
        IQueryable<Foundation> GetFoundations();
        bool UpdateFoundation(Foundation data);
        IQueryable<Foundation> GetFoundation(string dataId);
    }
}
