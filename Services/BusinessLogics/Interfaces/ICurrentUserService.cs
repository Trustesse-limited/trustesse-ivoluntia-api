
namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface ICurrentUserService
    {
        Task<string> GetUserFoundationId(string userId);
        string GetUserId();
    }
}
