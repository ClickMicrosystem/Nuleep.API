using System.Collections.Generic;
using System.Security.Claims;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Models;
using Nuleep.Models.Blogs;
using Nuleep.Models.Request;
using Nuleep.Models.Response;
using Stripe;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly AzureFileService _azurefileService;

        public OrganizationController(IOrganizationService organizationService, AzureFileService azurefileService)
        {
            _organizationService = organizationService;
            _azurefileService = azurefileService;
        }       

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

        [HttpPost]
        public async Task<IActionResult> CreateOrganization([FromBody] Organization orgRequest)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();

            var result = await _organizationService.CreateOrganization(userId, orgRequest);
            if (!result.Success)
                return BadRequest(new { error = result.ErrorMessage });

            return Ok(new { success = true, data = result.Data });
        }

        [HttpPut("{orgId}")]
        public async Task<IActionResult> EditOrganization(int orgId, [FromBody] Organization orgRequest)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();

            try
            {
                var updatedOrg = await _organizationService.EditOrganization(orgId, userId, orgRequest);
                return Ok(new { success = true, data = updatedOrg });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }

        [HttpPost("validate")]
        public async Task<IActionResult> ValidateOrgCode([FromBody] Organization request)
        {
            var organization = await _organizationService.GetByOrgCode(request.OrgCode);

            if (organization == null)
                return NotFound(new { error = "Organization not found!" });

            return Ok(new { success = true });
        }

        //[HttpPost("orgimage")]
        //public async Task<IActionResult> UpdateCompanyLogo([FromForm] int orgId, [FromForm] IFormFile file)
        //{
        //    try
        //    {
        //        var organization = await _organizationService.GetOrganizationById(orgId);
        //        if (organization.Id < 1)
        //            return NotFound(new { error = $"Organization not found with ID {orgId}" });

        //        if (!string.IsNullOrEmpty(organization.orgImage.BlobName))
        //        {
        //            await _azurefileService.DeleteByBlobNameAsync("orgimages", organization.orgImage.BlobName);
        //        }

        //        var uploadResult = await _azurefileService.UploadAsync("orgimages", file);

        //        var updatedOrganization = await _organizationService.UpdateOrganizationLogo(orgId, uploadResult.Data);
        //        return Ok(new { success = true, data = updatedOrganization });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { error = ex.Message });
        //    }
        //}

        [HttpPost("orgimageRemove")]
        public async Task<IActionResult> DeleteCompanyLogo([FromBody] DeleteCompanyLogoRequest request)
        {
            var organization = await _organizationService.GetOrganizationById(request.OrgId);

            if (organization == null || organization.Id < 1)
                return NotFound(new { error = $"Organization not found with ID {request.OrgId}" });

            var deleteResult = await _azurefileService.DeleteByBlobNameAsync("orgimages", request.FileName);

            organization.orgImage = new MediaImage();

            var result = await _organizationService.UpdateOrganizationLogo(request.OrgId, organization.orgImage);

            return Ok(new { success = true, data = result });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetOrganizations()
        {
            var result = await _organizationService.GetAllOrganizationList();
            return Ok(new { success = true, data = result });
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinOrganization([FromBody] JoinOrganizationRequest request)
        {
            try
            {
                request.UserId = int.Parse(User.Claims.ToList()[0].Value);

                await _organizationService.JoinOrganization(request);
                return Ok(new { success = true });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { success = false, error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPost("{orgID}/approve")]
        public async Task<IActionResult> ApproveJoinOrganization(int orgId, [FromBody] ApproveJoinRequest request)
        {
            try
            {
                var UserId = int.Parse(User.Claims.ToList()[0].Value);

                await _organizationService.ApproveJoinOrganization(request.ProfileId, orgId, UserId, request.Role);

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, error = ex.Message });
            }
        }


    }
}
