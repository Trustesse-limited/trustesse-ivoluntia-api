using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Data.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<ApiResponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders);
    }
}
