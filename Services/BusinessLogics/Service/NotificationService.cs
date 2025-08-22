using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Data.DataContext;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.Services.BusinessLogics.Service
{
    public class NotificationService : INotificationService
    {
        private readonly iVoluntiaDataContext _context;

        public NotificationService(iVoluntiaDataContext context)
        {
            _context = context;
        }


        public async Task<ApiResponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders)
        {
            try
            {
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.NotificationType == notificationType
                                           && t.NotificationChannel == channel);

                if (template == null)
                    return ApiResponse<string>.Failure(StatusCodes.Status404NotFound, "Notification template not found.");

                string message = template.Template;

                if (placeholders != null)
                {
                    foreach (var item in placeholders)
                    {
                        string placeholder = $"[{item.Key}]";
                        message = message.Replace(placeholder, item.Value ?? string.Empty);
                    }
                }

                return ApiResponse<string>.Success("Notification composed successfully.", message);
            }
            catch (Exception ex)
            {
                return ApiResponse<string>.Failure(StatusCodes.Status500InternalServerError, $"An error occurred: {ex.Message}");
            }
        }

    }






}
