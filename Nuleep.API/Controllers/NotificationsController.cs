using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : Controller
    {

        private readonly INotificationsService _notificationsService;
        public NotificationsController(INotificationsService notificationsService)
        {
            _notificationsService = notificationsService;
        }

        [HttpPost("get")]
        public async Task<IActionResult> GetNotifications([FromBody] GetNotificationsRequest request)
        {
            var notifications = await _notificationsService.GetNotifications(request);
            return Ok(new { success = true, data = new { data = notifications } });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNotification([FromBody] AddNotificationRequest request)
        {
            var notification = new Notification
            {
                Title = request.Title,
                UserId = request.UserId,
                RoomId = request.RoomId,
                NotificationType = "CHAT",
                IsRead = false
            };

            var result = await _notificationsService.AddNotification(notification);
            return Ok(new { success = true, data = result });
        }

        [HttpPost("read")]
        public async Task<IActionResult> ReadNotifications([FromBody] ReadNotificationRequest request)
        {
            var updated = await _notificationsService.MarkAsRead(request);
            return Ok(new { success = true, data = new { data = updated } });
        }
    }
}
