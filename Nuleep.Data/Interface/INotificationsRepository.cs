using Nuleep.Models;
using Nuleep.Models.Request;

namespace Nuleep.Data.Interface
{
    public interface INotificationsRepository
    {
        Task<IEnumerable<Notification>> GetNotifications(GetNotificationsRequest request);
        Task<Notification> AddNotification(Notification notification);
        Task<int> MarkAsRead(ReadNotificationRequest request);


    }
}
