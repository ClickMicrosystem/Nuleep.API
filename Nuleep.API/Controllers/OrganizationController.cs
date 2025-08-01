using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Response;

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

        [HttpPost("organization/validate")]
        [Authorize]
        public async Task<IActionResult> ValidateOrgCode([FromBody] Organization request)
        {
            var organization = await _organizationService.GetByOrgCode(request.OrgCode);

            if (organization == null)
                return NotFound(new { error = "Organization not found!" });

            return Ok(new { success = true });
        }


        // @desc      Get all information for employee organization
        // @route     GET /api/organizations/
        // @access    Private

        [HttpPost("GetEmployeeOrganization")]
        public async Task<IActionResult> GetEmployeeOrganization([FromQuery] int page = 0, [FromQuery] int limit = 10)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();

            var orgnaizationData = await _organizationService.GetEmployeeOrganization(page, limit, userId);

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

        [HttpGet("{orgID}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOrganization(int orgID)
        {
            OrganizationsResponse organization = await _organizationService.GetOrganizationById(orgID);
            if (organization.Id < 1)
            {
                return NotFound(new { error = $"Organization not found with the id of {orgID}" });
            }

            var jobs = await _organizationService.GetJobsByOrganizationId(orgID);

            var response = new GetOrganizationResponse
            {
                Success = true,
                Count = jobs.Count,
                Data = new
                {
                    organization,
                    jobs
                }
            };

            return Ok(response);
        }


    }
}
