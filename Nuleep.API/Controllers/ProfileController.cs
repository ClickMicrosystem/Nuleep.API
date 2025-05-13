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
using Microsoft.IdentityModel.Tokens;
using Nuleep.Business.Interface;
using Nuleep.Business.Services;
using Nuleep.Data.Interface;
using Nuleep.Data.Repository;
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
        private readonly AzureFileService _fileService;
        private readonly IProfileRepository _profileRepository;

        public ProfilesController(IProfileService profileService, AzureFileService fileService, IProfileRepository profileRepository)
        {
            _profileService = profileService;
            _fileService = fileService;
            _profileRepository = profileRepository;
        }

        // @desc      Get your own profile. This will check your token for the profile id
        // @route     GET /api/profiles
        // @access    Private
        [HttpGet]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _profileService.GetProfileByUsernameAsync(userId);

            if (user == null)
                return NotFound();

            return Ok(new { success = true, data = user });
        }


        // @desc      Create a new profile
        // @route     POST /api/profiles
        // @access    Private

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateProfile([FromBody] CreateOrUpdateProfileRequest profileRequest)
        {

            profileRequest.UserId = int.Parse(User.Claims.ToList()[0].Value);

            Recruiter recruiter = new Recruiter();
            JobSeeker jobSeeker = new JobSeeker();


            if (profileRequest.Role?.ToLower() != "jobseeker" && profileRequest.Role?.ToLower() != "recruiter")
            {
                return NotFound(new { error = "Invalid role" });
            }

            if (profileRequest.Role?.ToLower() == "recruiter")
            {
                recruiter = await _profileService.CreateRecruiterProfile(profileRequest);
                return Ok(new { success = true, data = recruiter });

            }
            else
            {
                jobSeeker = await _profileService.CreateJobSeekerProfile(profileRequest);
                return Ok(new { success = true, data = jobSeeker });
            }

            //if (createdProfileInfo.code == 1)
            //{
            //    return BadRequest(new { error = "Profile already exists!" });
            //}



        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] CreateOrUpdateProfileRequest request)
        {
            request.UserId = int.Parse(User.Claims.ToList()[0].Value);

            Recruiter recruiter = new Recruiter();
            JobSeeker jobSeeker = new JobSeeker();


            if (request.Role?.ToLower() != "jobseeker" && request.Role?.ToLower() != "recruiter")
            {
                return NotFound(new { error = "Invalid role" });
            }

            if (request.Role?.ToLower() == "recruiter")
            {
                recruiter = await _profileService.CreateRecruiterProfile(request);
                return Ok(new { success = true, data = recruiter });

            }
            else
            {
                jobSeeker = await _profileService.CreateJobSeekerProfile(request);
                return Ok(new { success = true, data = jobSeeker });
            }

            //var updatedProfile = await _profileService.UpdateProfile(request);

            //if (updatedProfile.code == 1)
            //{
            //    return BadRequest(new { error = "Profile does not exist!" });
            //}
            //else if (updatedProfile.code == 2)
            //{
            //    return Unauthorized(new { error = "You are not authorized to edit this profile!" });
            //}
            //else if (updatedProfile.code == 3)
            //{
            //    return NotFound(new { error = "Invalid role" });
            //}
            //else if (updatedProfile.code == 4)
            //{
            //    return BadRequest(new { error = "Error fetching updated profile" });
            //}

            //return Ok(new
            //{
            //    success = true,
            //    data = updatedProfile
            //});
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UploadResume([FromForm] IFormFile file)
        {
            var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();
            var jobSeeker = await _profileService.GetProfileByUsernameAsync(userId);
            //var jobSeeker = await _profileRepository.GetJobSeekerByUserIdAsync(userId);

            if (jobSeeker == null)
                return NotFound(new { error = "Profile not found" });

            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".pages", ".txt" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
            {
                return StatusCode(405, new
                {
                    success = false,
                    data = new { error = "File type must be .pdf, .doc, .txt, .pages" }
                });
            }

            // Delete old resume if exists
            if (!string.IsNullOrEmpty(jobSeeker.ResumeBlobName))
            {
                var deleted = await _fileService.DeleteFileAsync(jobSeeker.ResumeBlobName);
                if (deleted)
                {
                    await _profileService.RemoveResumeReferenceAsync(jobSeeker.Id);
                }
            }

            // Upload new resume
            var uploadResult = await _fileService.UploadFileAsync(file);
            if (uploadResult.Success)
            {
                await _profileService.SaveResumeAsync(jobSeeker.Id, file.FileName, uploadResult.BlobName, uploadResult.FullUrl);

                return Ok(new
                {
                    success = true,
                    data = await _profileService.GetProfileByUsernameAsync(userId)
                });
            }

            return StatusCode(500, new { success = false, error = "Resume upload failed" });
        }

    }
}
