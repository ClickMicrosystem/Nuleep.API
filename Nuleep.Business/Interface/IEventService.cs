using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IEventService
    {
        Task<Events> AddEvent(EventEditCreateRequest request);
        Task<PagedResult<Events>> ListAllEvents(EventListRequest request);
        Task<PagedResult<Events>> GetJobSeekerEvents(JobSeekerEventFilterRequest request);
        Task<bool> SoftDeleteEvent(int id);
        Task<object?> GetEventDetails(int id);
        Task<List<string>> GetEventTags();
        Task<bool> EditEvent(EventEditCreateRequest request);
        Task<bool> RegisterUserForEvent(EventRegisterRequest request);
        Task<bool> UnregisterUserFromEvent(EventUnregisterRequest request);
        Task<string> CreateStripeCheckoutSession(EventCheckoutRequest request);
        Task<bool> RefundEventPayment(string paymentIntentId);

    }
}
