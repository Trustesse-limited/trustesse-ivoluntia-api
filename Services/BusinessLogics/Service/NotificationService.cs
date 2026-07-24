using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Trustesse.Ivoluntia.Commons.Extensions.Helpers;
using Trustesse.Ivoluntia.Commons.Models.Response;
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

        public async Task<GlobalRequestReponse<string>> ComposeNotificationAsync(string notificationType, string channel, Dictionary<string, string> placeholders)
        {
            try
            {
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(t => t.NotificationType == notificationType && t.NotificationChannel == channel);

                if (template == null)
                    return ResponseHelper.BuildResponse<string>("Notification template not found.", StatusCodes.Status404NotFound, null, false);

                string message = template.Template;

                if (placeholders != null)
                {
                    foreach (var item in placeholders)
                    {
                        string placeholder = $"[{item.Key}]";
                        message = message.Replace(placeholder, item.Value ?? string.Empty);
                    }
                }
                return ResponseHelper.BuildResponse("Notification composed successfully.", StatusCodes.Status200OK, message, true);
            }
            catch (Exception ex)
            {
                return ResponseHelper.BuildResponse<string>($"An error occurred: {ex.Message}", StatusCodes.Status500InternalServerError, null, false);
            }
        }
    }
}
