using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface ICurrentUserRepository
    {
        Task<ApiResponse<string>> GetUserFoundationId(string userId);
        string GetUserId();
        string GetUserEmail();
        string GetUserFirstName();
    }
}
