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

        //[HttpGet("profiles")]
        //[Authorize]
        //public async Task<IActionResult> getProfile()
        //{
        //    // Get user ID from JWT claims
        //    var userIdClaim = int.Parse(User.Claims.ToList()[0].Value);

        //    //if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        //    //    return Unauthorized(new { error = "Invalid user token." });

        //    // Get profile by userRef
        //    var profileQuery = "SELECT * FROM Profiles WHERE UserRefId = @UserId AND IsDelete = 0;";
        //    var profile = await _db.QueryFirstOrDefaultAsync<ProfileDto>(profileQuery, new { UserId = userId });

        //    if (profile == null)
        //        return NotFound(new { error = "Profile not found" });

        //    // Fetch related data
        //    var userQuery = @"SELECT Id, Email, Name, Role, Subscription FROM Users WHERE Id = @UserId";
        //    var userRef = await _db.QueryFirstOrDefaultAsync<UserDto>(userQuery, new { UserId = userId });

        //    var orgQuery = @"SELECT Id, Name FROM Organizations WHERE Id = @OrgId";
        //    var organization = await _db.QueryFirstOrDefaultAsync<OrganizationDto>(orgQuery, new { OrgId = profile.OrganizationId });

        //    var savedJobsQuery = @"
        //SELECT j.Id, j.Title, j.CompanyName 
        //FROM SavedJobs sj
        //JOIN Jobs j ON sj.JobId = j.Id
        //WHERE sj.ProfileId = @ProfileId";
        //    var savedJobs = (await _db.QueryAsync<JobDto>(savedJobsQuery, new { ProfileId = profile.Id })).ToList();

        //    var recentJobsQuery = @"
        //SELECT j.Id, j.Title, j.CompanyName 
        //FROM RecentlyViewedJobs rv
        //JOIN Jobs j ON rv.JobId = j.Id
        //WHERE rv.ProfileId = @ProfileId";
        //    var recentlyViewedJobs = (await _db.QueryAsync<JobDto>(recentJobsQuery, new { ProfileId = profile.Id })).ToList();

        //    // Chatrooms with nested user profiles
        //    var chatRoomsQuery = @"
        //SELECT c.Id, c.RoomName, c.Image 
        //FROM ProfileChatrooms pc
        //JOIN Chatrooms c ON c.Id = pc.ChatroomId
        //WHERE pc.ProfileId = @ProfileId";
        //    var chatRooms = (await _db.QueryAsync<ChatRoomDto>(chatRoomsQuery, new { ProfileId = profile.Id })).ToList();

        //    foreach (var chatRoom in chatRooms)
        //    {
        //        var usersInRoomQuery = @"
        //    SELECT p.Id, p.FirstName, p.LastName
        //    FROM ProfileChatrooms pc
        //    JOIN Profiles p ON pc.ProfileId = p.Id
        //    WHERE pc.ChatroomId = @RoomId";
        //        chatRoom.Users = (await _db.QueryAsync<ChatUserDto>(usersInRoomQuery, new { RoomId = chatRoom.Id })).ToList();

        //        foreach (var user in chatRoom.Users)
        //        {
        //            var imgQuery = "SELECT TOP 1 FullUrl FROM ProfileImages WHERE ProfileId = @ProfileId";
        //            user.ProfileImg = await _db.QueryFirstOrDefaultAsync<string>(imgQuery, new { ProfileId = user.Id });
        //        }
        //    }

        //    // Return structured response
        //    var response = new
        //    {
        //        profile.Id,
        //        profile.FirstName,
        //        profile.LastName,
        //        profile.Email,
        //        profile.JobTitle,
        //        profile.Phone,
        //        profile.CreatedAt,
        //        UserRef = userRef,
        //        Organization = organization,
        //        RecentlyViewedJobs = recentlyViewedJobs,
        //        SavedJobs = savedJobs,
        //        ChatRooms = chatRooms
        //    };

        //    return Ok(new { success = true, data = response });
        //}


        //[HttpPost("api/profiles")]
        //public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
        //{
        //    using (var connection = new SqlConnection(_connectionString))
        //    {
        //        // Query user so that we can update the role
        //        var user = await connection.QuerySingleOrDefaultAsync<User>("SELECT * FROM Users WHERE Id = @Id", new { Id = request.UserId });

        //        // Find if profile already exists
        //        var profile = await connection.QuerySingleOrDefaultAsync<Profile>("SELECT * FROM Profiles WHERE UserRef = @UserRef", new { UserRef = request.UserId });

        //        if (profile != null) return BadRequest(new { error = "Profile already exists!" });

        //        Profile schema;
        //        if (request.Role == "jobSeeker")
        //        {
        //            schema = new JobSeekerProfile
        //            {
        //                UserRef = request.UserId,
        //                Email = user.Email,
        //                // Map other properties from request
        //            };
        //        }
        //        else if (request.Role == "recruiter")
        //        {
        //            schema = new RecruiterProfile
        //            {
        //                UserRef = request.UserId,
        //                Email = user.Email,
        //                // Map other properties from request
        //            };
        //        }
        //        else
        //        {
        //            return NotFound(new { error = "Invalid role" });
        //        }

        //        // Save profile
        //        await connection.ExecuteAsync("INSERT INTO Profiles (UserRef, Email, ...) VALUES (@UserRef, @Email, ...)", schema);

        //        user.Role = request.Role;
        //        await connection.ExecuteAsync("UPDATE Users SET Role = @Role WHERE Id = @Id", new { Role = user.Role, Id = user.Id });

        //        return Ok(new { success = true, data = schema });
        //    }
        //}

    }
}
