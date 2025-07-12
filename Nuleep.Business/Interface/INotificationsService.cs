using Nuleep.Models;
using Nuleep.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface INotificationsService
    {
        Task<IEnumerable<Notification>> GetNotifications(GetNotificationsRequest request);
        Task<Notification> AddNotification(Notification notification);
        Task<int> MarkAsRead(ReadNotificationRequest request);

    }
}
