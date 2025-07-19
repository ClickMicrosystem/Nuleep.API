using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Models.Request;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/event")]
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly AzureFileService _azurefileService;

        public EventController(IEventService eventService, AzureFileService azureFileService)
        {
            _eventService = eventService;
            _azurefileService = azureFileService;
        }

        [HttpPost("addEvent")]
        public async Task<IActionResult> AddEvent([FromBody] EventEditCreateRequest request)
        {
            try
            {
                var result = await _eventService.AddEvent(request);
                return Ok(new { success = false, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("adminListAllEvent")]
        public async Task<IActionResult> ListAllEvents([FromBody] EventListRequest request)
        {
            try
            {
                var result = await _eventService.ListAllEvents(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("jobSeekerListAllEvent")]
        public async Task<IActionResult> JobSeekerListEvent([FromBody] JobSeekerEventFilterRequest request)
        {
            try
            {
                var result = await _eventService.GetJobSeekerEvents(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost("imageUpload")]
        [Authorize]
        public async Task<IActionResult> EditEventImage([FromForm] FilePayload filePayload)
        {
            if (filePayload.File == null || filePayload.File.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            var result = await _azurefileService.UploadAsync("events", filePayload.File);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    data = result.Data
                });
            }

            return StatusCode(500, new { success = false, error = "Upload Failed" });
        }

        [HttpPost("delete")]
        public async Task<IActionResult> DeleteEvent([FromBody] EventDeleteRequest request)
        {
            var deleted = await _eventService.SoftDeleteEvent(request.Id);
            if (!deleted)
                return NotFound(new { error = "Event not found!" });

            return Ok(new { success = true });
        }

        [HttpPost("eventFetch")]

        public async Task<IActionResult> GetEventDetails([FromBody] EventDetailRequest request)
        {
            var result = await _eventService.GetEventDetails(request.Id);
            if (result == null)
                return NotFound(new { error = "Event not found!" });

            return Ok(new { success = true, data = result });
        }

        [HttpGet("getEventTags")]
        public async Task<IActionResult> GetEventTags()
        {
            var tags = await _eventService.GetEventTags();

            if (tags == null || !tags.Any())
                return NotFound(new { error = "No tags found!" });

            return Ok(new { success = true, data = tags });
        }

        [HttpPost("edit")]
        public async Task<IActionResult> EditEvent([FromBody] EventEditCreateRequest request)
        {
            var updated = await _eventService.EditEvent(request);
            if (!updated)
                return NotFound(new { error = "Event not found!" });

            return Ok(new { success = true });
        }

        [HttpPost("eventUserRegister")]
        public async Task<IActionResult> RegisterUser([FromBody] EventRegisterRequest request)
        {
            try
            {
                var success = await _eventService.RegisterUserForEvent(request);

                if (!success)
                    return NotFound(new { error = "Event not found or already deleted" });

                return Ok(new { message = "Registered for event successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("eventUserUnRegister")]
        public async Task<IActionResult> UnregisterUser([FromBody] EventUnregisterRequest request)
        {
            try
            {
                var result = await _eventService.UnregisterUserFromEvent(request);
                if (!result)
                    return NotFound(new { error = "Event not found or user not registered" });

                return Ok(new { message = "Unregistered from event successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> EventCheckOut([FromBody] EventCheckoutRequest request)
        {
            try
            {
                var sessionUrl = await _eventService.CreateStripeCheckoutSession(request);
                return Ok(new { url = sessionUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("eventRefund")]
        public async Task<IActionResult> RefundEvent([FromBody] EventRefundRequest request)
        {
            try
            {
                var success = await _eventService.RefundEventPayment(request.Pid);

                if (!success)
                    return BadRequest(new { status = false, error = "Refund not processed" });

                return Ok(new { status = true });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { status = false, error = ex.Message });
            }
        }



    }
}
