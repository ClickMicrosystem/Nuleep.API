using Microsoft.AspNetCore.Mvc;

namespace Nuleep.API.Controllers
{
    [Route("/")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok("API is running!");
        }
    }
}
