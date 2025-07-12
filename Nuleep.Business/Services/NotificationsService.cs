using Azure.Core;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;

namespace Nuleep.Business.Services
{
    public class NotificationsService : INotificationsService
    {
        
        private readonly INotificationsRepository _notificationsRepository;

        public NotificationsService(INotificationsRepository notificationsRepository)
        {
            _notificationsRepository = notificationsRepository;
        }

        public async Task<IEnumerable<Notification>> GetNotifications(GetNotificationsRequest request)
        {
            return await _notificationsRepository.GetNotifications(request);
        }

        public async Task<Notification> AddNotification(Notification notification)
        {
            return await _notificationsRepository.AddNotification(notification);
        }

        public async Task<int> MarkAsRead(ReadNotificationRequest request)
        {
            return await _notificationsRepository.MarkAsRead(request);
        }
    }
}
