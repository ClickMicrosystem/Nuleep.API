using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text;
using Azure.Core;
using Dapper;
using DocumentFormat.OpenXml.Office2016.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
using UglyToad.PdfPig;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Nuleep.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public partial class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly AzureFileService _azurefileService;
        private readonly IProfileRepository _profileRepository;

        public ProfilesController(IProfileService profileService, AzureFileService azurefileService, IProfileRepository profileRepository)
        {
            _profileService = profileService;
            _azurefileService = azurefileService;
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


        [HttpGet("{profileId}")]
        [AllowAnonymous]
        public async Task<IActionResult> ViewProfile(int profileId)
        {
            //var UserId = int.Parse(User.Claims.ToList()[0].Value);
            var profile = await _profileService.ViewProfile(profileId);

            if (profile == null)
                return NotFound(new { error = "Profile not found" });

            return Ok(new { success = true, data = profile });
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

        [HttpDelete]
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

        //[HttpPost("api/uploadResume")]
        //[Authorize]
        //public async Task<IActionResult> UploadResume([FromForm] IFormFile file)
        //{
        //    var userId = int.Parse(User.Claims.ToList()[0].Value).ToString();
        //    var jobSeeker = await _profileService.GetProfileByUsernameAsync(userId);

        //    if (jobSeeker == null)
        //        return NotFound(new { error = "Profile not found" });

        //    var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".pages", ".txt" };
        //    var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        //    if (!allowedExtensions.Contains(ext))
        //    {
        //        return StatusCode(405, new
        //        {
        //            success = false,
        //            data = new { error = "File type must be .pdf, .doc, .txt, .pages" }
        //        });
        //    }

        //    // Delete old resume if exists
        //    if (!string.IsNullOrEmpty(jobSeeker.ResumeBlobName))
        //    {
        //        var deleted = await _azurefileService.DeleteAsync("resumes", jobSeeker.ResumeBlobName);
        //        if (deleted)
        //        {
        //            await _profileService.RemoveResumeReferenceAsync(jobSeeker.Id);
        //        }
        //    }

        //    // Upload new resume
        //    var uploadResult = await _azurefileService.UploadAsync("resumes", file);
        //    if (uploadResult.Success)
        //    {
        //        await _profileService.SaveResumeAsync(jobSeeker.Id, file.FileName, uploadResult.Data.BlobName, uploadResult.Data.FullUrl);

        //        return Ok(new
        //        {
        //            success = true,
        //            data = await _profileService.GetProfileByUsernameAsync(userId)
        //        });
        //    }

        //    return StatusCode(500, new { success = false, error = "Resume upload failed" });
        //}

        

        [HttpPost("profileImg")]
        [Authorize]
        public async Task<IActionResult> EditProfileImage(MediaPayloadWithPId mediaPayloadWithPId)
        {
            var profile = await _profileService.ViewProfile(mediaPayloadWithPId.ProfileId);
            if (profile == null)
                return BadRequest(new { error = "Profile does not exist!" });

            // Upload new header image
            var uploadResult = await _azurefileService.UploadAsync("profileimages", mediaPayloadWithPId.File);
            var updatedProfile = "";
            if (uploadResult.Success)
            {
                updatedProfile = await _profileService.UpdateProfileImage(mediaPayloadWithPId.ProfileId, uploadResult.Data);
            }

            //var updatedProfile = await _profileService.ViewProfile(mediaPayloadWithPId.ProfileId);
            return Ok(new { success = true, data = updatedProfile });
        }

        [HttpPost("headerImg")]
        [Authorize]
        public async Task<IActionResult> EditHeaderImage(MediaPayloadWithPId mediaPayloadWithPId)
        {
            var profile = await _profileService.ViewProfile(mediaPayloadWithPId.ProfileId);
            if (profile == null)
                return BadRequest(new { error = "Profile does not exist!" });


            // Upload new header image
            var uploadResult = await _azurefileService.DeleteAsync("headerimages", mediaPayloadWithPId.ProfileId);

            // Upload new image
            var newHeaderImage = await _azurefileService.UploadAsync("headerimages", mediaPayloadWithPId.File);
            if (newHeaderImage == null)
                return StatusCode(500, new { error = "Upload failed" });

            var updatedProfile = "";
            if (newHeaderImage.Success)
            {
                updatedProfile = await _profileService.UpdateHeaderImage(mediaPayloadWithPId.ProfileId, newHeaderImage.Data);
            }

            return Ok(new { success = true, data = updatedProfile });
        }


        [HttpPost("fileTotext")]
        [Authorize]
        public async Task<IActionResult> FileToText(IFormFile file)
        {
            await Task.Delay(1);
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".pdf" && extension != ".docx")
            {
                return BadRequest(new { error = "Only .docx and .pdf format allowed!" });
            }

            string extractedText;

            using var stream = file.OpenReadStream();
            if (extension == ".pdf")
            {
                extractedText = ExtractTextFromPdf(stream);
            }
            else if (extension == ".docx")
            {
                extractedText = ExtractTextFromDocx(stream);
            }
            else
            {
                return BadRequest(new { error = "Unsupported file type" });
            }

            return Ok(new
            {
                success = true,
                data = extractedText
            });
        }

        private string ExtractTextFromPdf(Stream stream)
        {
            var textBuilder = new StringBuilder();
            using var pdf = PdfDocument.Open(stream);
            foreach (var page in pdf.GetPages())
            {
                textBuilder.AppendLine(page.Text);
            }
            return textBuilder.ToString();
        }

        private string ExtractTextFromDocx(Stream stream)
        {
            var textBuilder = new StringBuilder();
            using var wordDoc = WordprocessingDocument.Open(stream, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                textBuilder.Append(body.InnerText);
            }
            return textBuilder.ToString();
        }

        [HttpPost("images")]
        [Authorize]
        public async Task<IActionResult> FindImg([FromBody] FindImgRequest request)
        {
            if (string.IsNullOrEmpty(request.ContainerName))
                return BadRequest(new { error = "Container name is required" });

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var jobSeeker = await _profileRepository.GetExistingProfileByUserAsync(userId.ToString());

            if (jobSeeker == null)
                return NotFound(new { error = "Profile not found" });

            var result = await _azurefileService.FindAsync(request.ContainerName);

            return Ok(new
            {
                success = true,
                result
            });
        }


        //[HttpPost("projectImageUpload")]
        //[Authorize]
        //public async Task<IActionResult> ProjectImgUpload([FromForm] IFormFile file,[FromForm] int? removeName)
        //{
        //    var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        //    var jobSeeker = await _profileRepository.GetExistingProfileByUserAsync(userId.ToString());

        //    if (jobSeeker == null)
        //        return NotFound(new { error = "Profile not found" });

        //    var uploadResult = await _azurefileService.UploadAsync("projectimg", file);

        //    if (!uploadResult.Success)
        //        return BadRequest(new { error = "File upload failed" });

        //    var uploadedData = uploadResult.Data;

        //    // Deserialize existing project images (assuming it's stored as JSON or List<BlobResultDto>)
        //    var projectImages = jobSeeker.ProjectImage ?? new List<ProjectImage>();

        //    if (removeName.HasValue && removeName.Value >= 0 && removeName.Value < projectImages.Count)
        //    {
        //        var toRemove = projectImages[removeName.Value];

        //        await _azurefileService.DeleteAsync("projectimg", toRemove.BlobName);

        //        projectImages[removeName.Value] = uploadedData;
        //    }
        //    else
        //    {
        //        projectImages.Add(uploadedData);
        //    }

        //    jobSeeker.ProjectImage = projectImages;

        //    //await _profileRepository.Upl(userId, projectImages);

        //    return Ok(new
        //    {
        //        success = true,
        //        data = jobSeeker
        //    });
        //}

        public class MediaPayloadWithPId
        {
            [FromForm(Name = "pid")]
            public int ProfileId { get; set; }

            [FromForm(Name = "file")]
            public IFormFile File { get; set; }
        }

        public class FilePayload
        {
            [Required]
            [FromForm(Name = "file")]
            public IFormFile File { get; set; }
        }
    }

    public class FindImgRequest
    {
        public string? ContainerName { get; set; }
    }
}
