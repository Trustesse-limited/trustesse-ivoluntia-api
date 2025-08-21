using Microsoft.AspNetCore.Mvc;
using Trustesse.Ivoluntia.Commons.DTOs;
using Trustesse.Ivoluntia.Commons.DTOs.DataTransferObjects;
using Trustesse.Ivoluntia.Services.Abstractions;

namespace Trustesse.Ivoluntia.API.Controllers.v1
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("compose")]
        public async Task<IActionResult> Compose([FromBody] ComposeNotificationDto request)
        {
            if (request == null)
                return BadRequest(ApiResponse<string>.Failure(StatusCodes.Status400BadRequest, "Invalid request."));

            var response = await _notificationService.ComposeNotificationAsync(
                request.NotificationType,
                request.NotificationChannel,
                request.Placeholders
            );

            if (response.StatusCode != StatusCodes.Status200OK)
                return StatusCode(response.StatusCode, response);

            return Ok(response);
        }






    }
}
