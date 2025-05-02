using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Models.Blogs;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;

        public OrganizationController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
        }

        // @desc      Get all information for employee organization
        // @route     GET /api/organizations/
        // @access    Private

        [HttpPost("listAllBlog")]
        public async Task<IActionResult> GetEmployeeOrganization([FromQuery] int page = 0, [FromQuery] int limit = 10)
        {
            var orgnaizationData = await _organizationService.GetEmployeeOrganization(page, limit);


            if (orgnaizationData.code == 1)
            {
                return NotFound(new { error = "Profile not found" });
            }
            else if (orgnaizationData.code == 2)
            {
                return NotFound(new { error = "Organization not found" });
            }
            else if (orgnaizationData.code == 3)
            {
                return BadRequest(new { error = "You are not allowed to view this organization" });
            }

            return Ok(new { success = true, data = orgnaizationData });
        }
    }
}
