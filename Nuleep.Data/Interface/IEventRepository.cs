using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;

namespace Nuleep.Data.Interface
{
    public interface IEventRepository
    {
        Task<Events> AddEvent(EventEditCreateRequest request);
        Task<List<Events>> GetEvents(bool? isDelete, int limit, int offset);
        Task<int> GetEventsCount(bool? isDelete);

        Task<List<Events>> GetJobSeekerEvents(JobSeekerEventFilterRequest request);
        Task<int> GetJobSeekerEventsCount(JobSeekerEventFilterRequest request);
        Task<bool> SoftDeleteEvent(int id);
        Task<Events?> GetEventById(int id);
        Task<List<RegisteredUser>> GetRegisteredUsers(int eventId);
        Task<List<string>> GetEventTags();
        Task<bool> EditEvent(EventEditCreateRequest request);
        Task<bool> RegisterUserForEvent(int eventId, int userId);
        Task<bool> UnregisterUserFromEvent(int eventId, int userId);





    }
}
