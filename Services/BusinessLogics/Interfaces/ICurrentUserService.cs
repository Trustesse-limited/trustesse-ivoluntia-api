
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface ICurrentUserService
    {
        string GetUserId();
        string GetUserEmail();
        string GetUserFirstName();
        string GetUserFoundationId();
    }
}
