using Trustesse.Ivoluntia.Commons.DTOs;

namespace Trustesse.Ivoluntia.Services.Abstractions
{
    public interface INotificationService
    {
        Task<ApiResponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders);
    }
}
