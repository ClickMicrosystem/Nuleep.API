using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nuleep.Data.Interface;
using Azure.Core;

namespace Nuleep.Data.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IDbConnection _db;

        public ProfileRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task<dynamic> GetUserByUsernameAsync(string username)
        {
            var user = await _db.QueryAsync<int>("Select Id from Users where Email = @Email", new { Email = username });
            var id = user.ToList()[0];
            Recruiter recruiter = new Recruiter();
            try
            {

                var query = @"SELECT * FROM Profile WHERE UserId = @UserId;
                            SELECT Id, Email, Role FROM Users WHERE Id = @UserId;                            
                            SELECT Id, Email, Name FROM Organizations WHERE Id = (SELECT OrganizationId FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM Subscriptions WHERE UserId = @UserId;
                            SELECT * FROM Education WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM Awards WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM ProfileImages WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);";

                using var multi = await _db.QueryMultipleAsync(query, new { UserId = id });

                var rec = await multi.ReadFirstOrDefaultAsync<Recruiter>();
                var profile = await multi.ReadFirstOrDefaultAsync<Profile>();
                var users = await multi.ReadFirstOrDefaultAsync<User>();
                var organization = await multi.ReadFirstOrDefaultAsync<Organization>();
                var subscription = await multi.ReadFirstOrDefaultAsync<Subscription>();
                var education = (await multi.ReadAsync<Education>()).ToList();
                var awards = (await multi.ReadAsync<Award>()).ToList();
                //var profileImages = (await multi.ReadAsync<ProfileImage>()).ToList();

                if (rec != null)
                {
                    rec.savedCandidateIds = [];
                    rec.Education = education;
                    rec.Awards = awards;
                    rec.userRef = users;
                    rec.userRef.subscription = subscription;
                    rec.organization = organization;
                }

                recruiter = rec;

                //if (recruiter != null)
                //{

                //    recruiter.userRef = new User();
                //    recruiter.FirstName = profile.FirstName;
                //    recruiter.LastName = profile.LastName;
                //    //recruiter.OrganizationRole = profile.org;
                //    recruiter.Email = profile.Email;
                //    recruiter.CreatedAt = profile.CreatedAt;
                //    //recruiter.FirstName = profile.;
                //    recruiter.userRef.subscription = subscription;
                //    recruiter.userRef.Id = recruiter.Id;
                //    recruiter.userRef.Email = recruiter.Email;
                //    recruiter.userRef.Role = "jobseeker";
                //    recruiter.Education = education;
                //    recruiter.Awards = awards;
                //    //recruiter.ProfileImg = profileImages;
                //}


            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Error: {ex.Message}");
            }

            //var sql = "SELECT * FROM Users WHERE Email = @Email";
            //return await _db.QueryFirstOrDefaultAsync<User>(sql, new { Email = username });
            return recruiter;
        }
        public async Task<Profile> GetExistingProfileByUserAsync(string userId)
        {
            var sql = "SELECT * FROM Profile WHERE UserId = @UserRefId";
            return await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = userId });
        }
        public async Task<dynamic> CreateProfile(CreateProfileRequest profileRequest)
        {


            var sql = "SELECT * FROM Profile WHERE UserId = @UserRefId";
            var existingProfile =  await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = profileRequest.UserId });

            if (existingProfile != null)
            {
                return new { data = existingProfile, code = 1 };
            }

            string profileInsertQuery = @" INSERT INTO Recruiters (FirstName, LastName, Email, JobTitle, UserRef, Phone, About, Bio, StreetAddress, Title, OrganizationId, OrganizationRole, OrganizationApproved)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @UserRef, @Phone, @About, @Bio, @StreetAddress, @Title, @OrganizationId, @OrganizationRole, @OrganizationApproved);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

            var profileId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
            {
                profileRequest.FirstName,
                profileRequest.LastName,
                profileRequest.Email,
                profileRequest.JobTitle,
                UserRef = profileRequest.UserId,
                profileRequest.Phone,
                profileRequest.About,
                profileRequest.Bio,
                profileRequest.StreetAddress,
                profileRequest.Title,
                profileRequest.OrganizationId,
                profileRequest.OrganizationRole,
                profileRequest.OrganizationApproved
            });

            // Update User role
            await _db.ExecuteAsync("UPDATE Users SET Role = @Role WHERE Id = @Id", new
            {
                Role = profileRequest.Role.ToLower(),
                Id = profileRequest.UserId
            });

            var data = new { id = profileId, role = profileRequest.Role.ToLower(), code = 0 };
            return data;
            //return new { data = existingProfile, code = 1 };
        }

        private int GetLoggedInUserId()
        {
            return 2;
            //return int.Parse(User.FindFirst("id").Value); // assuming JWT has user id in claim "id"
        }

        public async Task<dynamic> UpdateProfile(ProfileRequest request)
        {
            var userId = GetLoggedInUserId();

            // 1. Find existing profile
            var profile = await _db.QueryFirstOrDefaultAsync<dynamic>(
                            "SELECT * FROM Profiles WHERE UserId = @UserId",
                            new { UserId = userId }
                        );

            if (profile == null)
            {
                return (new { data = profile, code = 1 });
            }

            // 2. Check ownership (this is actually already safe because we query by UserId)
            if (profile.UserId != userId)
            {
                return (new { data = profile, code = 2 });
            }

            // 3. Validate Role if Provided
            string profileType = profile.Type;
            if (!string.IsNullOrEmpty(request.Role))
            {
                if (request.Role.Equals("jobSeeker", StringComparison.OrdinalIgnoreCase))
                {
                    profileType = "jobSeeker";
                }
                else if (request.Role.Equals("recruiter", StringComparison.OrdinalIgnoreCase))
                {
                    profileType = "recruiter";
                }
                else
                {
                    return (new { data = profile, code = 3 });
                }
            }

            // 4. Update profile
            var updateQuery = @"
                                UPDATE Profiles
                                SET 
                                    FirstName = @FirstName,
                                    LastName = @LastName,
                                    Bio = @Bio,
                                    About = @About,
                                    Title = @Title,
                                    StreetAddress = @StreetAddress,
                                    Type = @Type
                                WHERE UserId = @UserId
                            ";

            await _db.ExecuteAsync(updateQuery, new
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Bio = request.Bio,
                About = request.About,
                Title = request.JobTitle,
                StreetAddress = request.StreetAddress,
                Type = profileType
            });

            // 5. Get updated profile (including nested data if needed)
            var updatedProfile = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
                                    SELECT 
                                        p.*, 
                                        u.Email, u.Role, u.Subscription, 
                                        o.Name as OrganizationName
                                    FROM Profiles p
                                    LEFT JOIN Users u ON p.UserId = u.Id
                                    LEFT JOIN Organizations o ON p.OrganizationId = o.Id
                                    WHERE p.UserId = @UserId
                                ", new { UserId = userId });

            if (updatedProfile == null)
            {
                return (new { data = profile, code = 4 });
            }

            return (new { data = updatedProfile, code = 0 });
        }
        

        public async Task<dynamic> DeleteProfile(int UserId)
        {
            var profile = await _db.QueryFirstOrDefaultAsync<Profile>(
                            "SELECT * FROM Profile WHERE UserRef = @UserId",
                            new { UserId = UserId });

            if (profile == null)
            {
                return (new { data = profile, code = 1 });
            }

            if (profile.UserId != UserId)
            {
                return (new { data = profile, code = 2 });
            }

            // Delete the profile
            await _db.ExecuteAsync(
                "DELETE FROM Profile WHERE UserRef = @UserId",
                new { UserId = UserId });

            return (new { data = profile, code = 0 });
        }



    }
}
