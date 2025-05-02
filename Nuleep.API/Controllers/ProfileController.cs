using System.Data;
using System.Net;
using System.Security.Claims;
using Azure.Core;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Business.Interface;
using Nuleep.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public partial class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var username = User.Identity?.Name;
            username = "rultilogni@vusra.com";
            if (string.IsNullOrEmpty(username))
                return Unauthorized();

            var user = await _profileService.GetProfileByUsernameAsync(username);
            if (user == null)
                return NotFound();

            return Ok(new { success = true, data = user });
        }


        // @desc      Create a new profile
        // @route     POST /api/profiles
        // @access    Private

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest profileRequest)
        {

            profileRequest.UserId = int.Parse(User.Claims.ToList()[0].Value);
            profileRequest.Email = User.Claims.ToList()[1].Value;

            if (profileRequest.Role?.ToLower() != "jobseeker" && profileRequest.Role?.ToLower() != "recruiter")
            {
                return NotFound(new { error = "Invalid role" });
            }

            var createdProfileInfo = await _profileService.CreateProfile(profileRequest);
            
            if (createdProfileInfo.code == 1)
            {
                return BadRequest(new { error = "Profile already exists!" });
            }



            return Ok(new { success = true, data = createdProfileInfo });
        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileRequest request)
        {

            var updatedProfile = await _profileService.UpdateProfile(request);

            if (updatedProfile.code == 1)
            {
                return BadRequest(new { error = "Profile does not exist!" });
            }
            else if (updatedProfile.code == 2)
            {
                return Unauthorized(new { error = "You are not authorized to edit this profile!" });
            }
            else if (updatedProfile.code == 3)
            {
                return NotFound(new { error = "Invalid role" });
            }
            else if (updatedProfile.code == 4)
            {
                return BadRequest(new { error = "Error fetching updated profile" });
            }

            return Ok(new
            {
                success = true,
                data = updatedProfile
            });
        }

        [HttpDelete("api/profiles")]
        [Authorize]
        public async Task<IActionResult> DeleteProfile()
        {
            var UserId = int.Parse(User.Claims.ToList()[0].Value);

            var deleteProfile = await _profileService.DeleteProfile(UserId);

            if (deleteProfile.code == 1)
            {
                return BadRequest(new { error = "Profile does not exist!" });
            }
            else if (deleteProfile.code == 2)
            {
                return Forbid("You are not authorized to delete this profile!");
            }

            return Ok(new { success = true, data = new { } });
        }

    }
}
