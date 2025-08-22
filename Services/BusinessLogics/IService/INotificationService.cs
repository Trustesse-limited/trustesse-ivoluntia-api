using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.IService
{
    public interface INotificationService
    {
        Task<ApiResponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders);
    }
}
