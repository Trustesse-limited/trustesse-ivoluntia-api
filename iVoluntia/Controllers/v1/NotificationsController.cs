using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.Models.Request;
using Trustesse.Ivoluntia.Services.BusinessLogics.IService;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("compose")]
        public async Task<IActionResult> Compose([FromBody] ComposeNotificationDto request)
        {
            request = request.Validate();

            return BuildHttpResponse(await _notificationService.ComposeNotificationAsync(
                request.NotificationType,
                request.NotificationChannel,
                request.Placeholders));
        }
    }
}
