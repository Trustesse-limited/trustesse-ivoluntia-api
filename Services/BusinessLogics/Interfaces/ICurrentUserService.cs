
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Interfaces
{
    public interface ICurrentUserService
    {
        Task<ApiResponse<string>> GetUserFoundationId(string userId);
        string GetUserId();
    }
}
