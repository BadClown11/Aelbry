using Aelbry.BL.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aelbry.Web.Controllers
{
    [Authorize]
    public class NotificationController : ApiControllerBase
    {
        private readonly NotificationBL _notificationBL;

        public NotificationController(NotificationBL notificationBL)
        {
            _notificationBL = notificationBL;
        }

        [HttpGet]
        [Authorize(Policy = "Permission:NOTIFICATIONS_USE")]
        public JsonResult GetMine(bool unreadOnly = false)
        {
            return Exec(() => _notificationBL.GetByUser(CurrentUserId, unreadOnly));
        }

        [HttpGet]
        [Authorize(Policy = "Permission:NOTIFICATIONS_USE")]
        public JsonResult GetUnreadCount()
        {
            return Exec(() => _notificationBL.GetUnreadCount(CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:NOTIFICATIONS_USE")]
        public JsonResult MarkAsRead(int notificationId)
        {
            return Exec(() => _notificationBL.MarkAsRead(notificationId, CurrentUserId));
        }

        [HttpPost]
        [Authorize(Policy = "Permission:NOTIFICATIONS_USE")]
        public JsonResult MarkAllAsRead()
        {
            return Exec(() => _notificationBL.MarkAllAsRead(CurrentUserId));
        }
    }
}
