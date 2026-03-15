using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Data.Repositories.Interfaces;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class NotificationService : INotificationService
    {
        public readonly INotificationRepository _notificatinRepository;
        public NotificationService(INotificationRepository notificatinRepository)
        {
            _notificatinRepository = notificatinRepository;
        }

        public async Task<ApiResponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders)
        {
            try
            {
                var response = await _notificatinRepository.ComposeNotificationAsync(notificationType, channel, placeholders);   

                return ApiResponse<string>.Success("Notification composed successfully.", response.Data);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }
    }
}
