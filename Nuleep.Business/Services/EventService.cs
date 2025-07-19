using Microsoft.Extensions.Configuration;
using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using Stripe.Checkout;
using Stripe;

namespace Nuleep.Business.Services
{
    public class EventsService : IEventService
    {
        
        private readonly IEventRepository _eventRepository;
        private readonly IConfiguration _config;

        public EventsService(IEventRepository eventRepository, IConfiguration config)
        {
            _eventRepository = eventRepository;
            _config = config;
        }

        public async Task<Events> AddEvent(EventEditCreateRequest request)
        {
            decimal? price = null;

            if (!string.IsNullOrEmpty(request.Price))
            {
                var priceSanitized = request.Price.Replace(",", "");
                if (decimal.TryParse(priceSanitized, out var parsedPrice))
                    price = parsedPrice;
            }

            request.Price = price.ToString();

            return await _eventRepository.AddEvent(request);
        }

        public async Task<PagedResult<Events>> ListAllEvents(EventListRequest request)
        {
            int page = request.Page > 0 ? request.Page : 1;
            int limit = request.Limit > 0 ? request.Limit : 10;
            int offset = (page - 1) * limit;

            var data = await _eventRepository.GetEvents(request.IsDelete, limit, offset);
            var total = await _eventRepository.GetEventsCount(request.IsDelete);

            return new PagedResult<Events>
            {
                Data = data,
                Total = total
            };
        }

        public async Task<PagedResult<Events>> GetJobSeekerEvents(JobSeekerEventFilterRequest request)
        {
            var events = await _eventRepository.GetJobSeekerEvents(request);
            var total = await _eventRepository.GetJobSeekerEventsCount(request);

            return new PagedResult<Events>
            {
                Data = events,
                Total = total
            };
        }

        public async Task<bool> SoftDeleteEvent(int id)
        {
            return await _eventRepository.SoftDeleteEvent(id);
        }

        public async Task<object?> GetEventDetails(int id)
        {
            var eventData = await _eventRepository.GetEventById(id);
            if (eventData == null) return null;

            var registeredUsers = await _eventRepository.GetRegisteredUsers(id);

            return new
            {
                Event = eventData,
                RegisterUsers = registeredUsers
            };
        }

        public async Task<List<string>> GetEventTags()
        {
            return await _eventRepository.GetEventTags();
        }
        public async Task<bool> EditEvent(EventEditCreateRequest request)
        {
            return await _eventRepository.EditEvent(request);
        }
        public async Task<bool> RegisterUserForEvent(EventRegisterRequest request)
        {
            return await _eventRepository.RegisterUserForEvent(request.EventId, request.UserId);
        }
        public async Task<bool> UnregisterUserFromEvent(EventUnregisterRequest request)
        {
            return await _eventRepository.UnregisterUserFromEvent(request.EventId, request.UserId);
        }


        public async Task<string> CreateStripeCheckoutSession(EventCheckoutRequest request)
        {
            var customerService = new CustomerService();
            var customers = await customerService.ListAsync(new CustomerListOptions
            {
                Email = request.Email,
                Limit = 1
            });

            var customerId = customers.Data.FirstOrDefault()?.Id;

            if (customerId == null)
            {
                var customer = await customerService.CreateAsync(new CustomerCreateOptions
                {
                    Email = request.Email
                });

                customerId = customer.Id;
            }

            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(new SessionCreateOptions
            {
                Customer = customerId,
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    Metadata = new Dictionary<string, string>
                                {
                                    { "userId", request.UserId.ToString() },
                                    { "eventId", request.EventId.ToString() }
                                }
                },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = "usd",
                            UnitAmount = (long)(request.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = request.EventName,
                                Images = new List<string> { request.ImageUrl }
                            }
                        }
                    }
                },
                Mode = "payment",
                SuccessUrl = $"{_config["Stripe:FrontendUrl"]}/event/{request.EventId}?success=true",
                CancelUrl = $"{_config["Stripe:FrontendUrl"]}/event/{request.EventId}"
            });

            return session.Url;
        }

        public async Task<bool> RefundEventPayment(string paymentIntentId)
        {
            var refundService = new RefundService();

            try
            {
                var refund = await refundService.CreateAsync(new RefundCreateOptions
                {
                    PaymentIntent = paymentIntentId
                });

                return refund.Status == "succeeded" || refund.Status == "pending";
            }
            catch (StripeException ex)
            {
                // You can log ex.StripeError.Message or return more details if needed
                throw new ApplicationException("Stripe refund failed", ex);
            }
        }


    }
}
