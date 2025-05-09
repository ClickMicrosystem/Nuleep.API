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
        

        private dynamic GetCreatedProfileData(string profileId)
        {
            var query = @"SELECT * FROM Profile WHERE UserId = @UserId;
                            SELECT Id, Email, Role FROM Users WHERE Id = @UserId;                            
                            SELECT Id, Email, Name FROM Organizations WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM Subscriptions WHERE UserId = @UserId;
                            SELECT * FROM Education WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM Awards WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);
                            SELECT * FROM ProfileImages WHERE ProfileId = (SELECT Id FROM Profile WHERE UserId = @UserId);";

            using var multi = _db.QueryMultiple(query, new { UserId = profileId });

            //var rec = multi.ReadFirstOrDefault<Recruiter>();
            var profile = multi.ReadFirstOrDefault<Profile>();
            var users = multi.ReadFirstOrDefault<User>();
            //var organization = multi.ReadFirstOrDefault<Organization>();
            //var subscription = multi.ReadFirstOrDefault<Subscription>();
            var education = (multi.Read<Education>()).ToList();
            var awards = (multi.Read<Award>()).ToList();
            //return 1;

            CreateProfileRequest createProfileRequest = new CreateProfileRequest();
            createProfileRequest.Awards = awards;
            createProfileRequest.FirstName = profile.FirstName;
            createProfileRequest.LastName = profile.LastName;
            createProfileRequest.Email = profile.Email;
            createProfileRequest.Role = users.Role;
            return createProfileRequest;
        }

        public async Task<dynamic> CreateProfile(CreateProfileRequest profileRequest)
        {

            ResponeModel responeModel = new ResponeModel();

            var sql = "SELECT Id FROM Profile WHERE UserId = @UserRefId";
            var existingProfileId =  await _db.QueryFirstOrDefaultAsync<int>(sql, new { UserRefId = profileRequest.UserId });

            if(profileRequest.Education.Count > 0)
            {
                foreach(var item in profileRequest.Education)
                {
                    if(item.Id == 0)
                    {
                        string educationInsertQuery = @"INSERT INTO Education (SchoolOrOrganization, DegreeCertification, FieldOfStudy, [From], [To], Present, ProfileId) VALUES (@SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @ProfileId);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var educationId = await _db.ExecuteScalarAsync<int>(educationInsertQuery, new
                        {
                            item.SchoolOrOrganization,
                            item.DegreeCertification,
                            item.FieldOfStudy,
                            item.From,
                            item.To,
                            item.Present,
                            ProfileId = existingProfileId
                        });
                    }
                    else
                    {
                        string educationUpdateQuery = @" UPDATE Education
                                                            SET
                                                                SchoolOrOrganization = @SchoolOrOrganization,
                                                                DegreeCertification = @DegreeCertification,
                                                                FieldOfStudy = @FieldOfStudy,
                                                                [From] = @From,
                                                                [To] = @To,
                                                                Present = @Present
                                                            WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(educationUpdateQuery, new
                        {
                            item.SchoolOrOrganization,
                            item.DegreeCertification,
                            item.FieldOfStudy,
                            item.From,
                            item.To,
                            item.Present,
                            item.Id,
                            ProfileId = existingProfileId
                        });


                    }
                }
            }

            if (profileRequest.Awards.Count > 0)
            {
                foreach (var item in profileRequest.Awards)
                {
                    if (item.Id == 0)
                    {
                        string awardInsertQuery = @" INSERT INTO Awards (AwardName, CompanyName, [Date], Description, ProfileId)
                                    VALUES (@AwardName, @CompanyName, @Date, @Description, @ProfileId);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var educationId = await _db.ExecuteScalarAsync<int>(awardInsertQuery, new
                        {
                            item.AwardName,
                            item.CompanyName,
                            item.Date,
                            item.Description,
                            ProfileId = existingProfileId
                        });
                    }
                    else
                    {
                        string educationUpdateQuery = @" UPDATE Awards
                                                            SET
                                                                AwardName = @AwardName,
                                                                CompanyName = @CompanyName,
                                                                Date = @Date,
                                                                Description = @Description
                                                            WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(educationUpdateQuery, new
                        {
                            item.AwardName,
                            item.CompanyName,
                            item.Date,
                            item.Description,
                            item.Id,
                            item.ProfileId
                        });


                    }
                }
            }            

            if(existingProfileId > 0 && (profileRequest.Awards.Count > 0 || profileRequest.Awards.Count > 0))
            {
                responeModel.data = GetCreatedProfileData(profileRequest.UserId.ToString());
                responeModel.code = 0;
                return responeModel;
            }

            if (existingProfileId > 0)
            {
                responeModel.data = existingProfileId;
                responeModel.code = 1;
                return responeModel;
            }

            if (profileRequest.Role?.ToLower() == "recruiter")
            {
                string profileInsertQuery = @" INSERT INTO Profile (FirstName, LastName, Email, JobTitle, isDelete, UserId, Phone)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @isDelete, @UserRef, @Phone);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var profileId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
                {
                    profileRequest.FirstName,
                    profileRequest.LastName,
                    profileRequest.Email,
                    profileRequest.JobTitle,
                    profileRequest.isDelete,
                    UserRef = profileRequest.UserId,
                    profileRequest.Phone
                });


                string recruitInsertQuery = @" INSERT INTO Recruiters (About, Bio, StreetAddress, Title, OrganizationRole, OrganizationApproved)
                                    VALUES (@About, @Bio, @StreetAddress, @Title, @OrganizationRole, @OrganizationApproved);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var recruitId = await _db.ExecuteScalarAsync<int>(recruitInsertQuery, new
                {
                    profileRequest.About,
                    profileRequest.Bio,
                    profileRequest.StreetAddress,
                    profileRequest.Title,
                    profileRequest.OrganizationRole,
                    profileRequest.OrganizationApproved
                });

            }
            else
            {
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
            }


            // Update User role
            await _db.ExecuteAsync("UPDATE Users SET Role = @Role WHERE Id = @Id", new
            {
                Role = profileRequest.Role.ToLower(),
                Id = profileRequest.UserId
            });

            responeModel.role = profileRequest.Role.ToLower();
            responeModel.data = 3;
            responeModel.code = 0;
            return responeModel;
        }

        public async Task<dynamic> UpdateProfile(CreateProfileRequest request)
        {
            ResponeModel responeModel = new ResponeModel();

            // 1. Find existing profile
            var profileId = await _db.QueryFirstOrDefaultAsync<int>(
                            "SELECT Id FROM Profile WHERE UserId = @UserId",
                            new { UserId = request.UserId }
                        );

            if (profileId < 1)
            {
                responeModel.data = profileId;
                responeModel.code = 1;
                return responeModel;
                //return (new { data = profileId, code = 1 });
            }

            // 3. Validate Role if Provided
            if(request.Role.ToLower() != "recruiter" && request.Role.ToLower() != "jobSeeker")
            {
                responeModel.data = profileId;
                responeModel.code = 3;
                return responeModel;
                //return (new { data = profileId, code = 3 });
            }


            if (request.Education.Count > 0)
            {
                foreach (var item in request.Education)
                {
                    if (item.Id == 0)
                    {
                        string educationInsertQuery = @" INSERT INTO Education (SchoolOrOrganization, DegreeCertification, FieldOfStudy, From, To, Present, ProfileId)
                                    VALUES (@SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @ProfileId);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var educationId = await _db.ExecuteScalarAsync<int>(educationInsertQuery, new
                        {
                            item.SchoolOrOrganization,
                            item.DegreeCertification,
                            item.FieldOfStudy,
                            item.From,
                            item.To,
                            item.Present,
                            ProfileId = profileId
                        });
                    }
                    else
                    {
                        string educationUpdateQuery = @" UPDATE Education
                                                            SET
                                                                SchoolOrOrganization = @SchoolOrOrganization,
                                                                DegreeCertification = @DegreeCertification,
                                                                FieldOfStudy = @FieldOfStudy,
                                                                [From] = @From,
                                                                [To] = @To,
                                                                Present = @Present
                                                            WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(educationUpdateQuery, new
                        {
                            item.SchoolOrOrganization,
                            item.DegreeCertification,
                            item.FieldOfStudy,
                            item.From,
                            item.To,
                            item.Present,
                            item.Id,
                            ProfileId = profileId
                        });


                    }
                }
            }

            if (request.Awards.Count > 0)
            {
                foreach (var item in request.Awards)
                {
                    if (item.Id == 0)
                    {
                        string awardInsertQuery = @" INSERT INTO Awards (AwardName, CompanyName, Date, Description, ProfileId)
                                    VALUES (@SchoolOrOrganization, @CompanyName, @Date, @Description, @ProfileId);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var educationId = await _db.ExecuteScalarAsync<int>(awardInsertQuery, new
                        {
                            item.AwardName,
                            item.CompanyName,
                            item.Date,
                            item.Description,
                            item.ProfileId
                        });
                    }
                    else
                    {
                        string educationUpdateQuery = @" UPDATE Awards
                                                            SET
                                                                AwardName = @AwardName,
                                                                CompanyName = @CompanyName,
                                                                Date = @Date,
                                                                Description = @Description
                                                            WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(educationUpdateQuery, new
                        {
                            item.AwardName,
                            item.CompanyName,
                            item.Date,
                            item.Description,
                            item.Id,
                            item.ProfileId
                        });


                    }
                }
            }

            if (profileId > 0)
            {
                responeModel.data = profileId;
                responeModel.code = 1;
                return responeModel;
                //return new { data = profileId, code = 1 };
            }

            if (request.Role?.ToLower() == "recruiter")
            {
                string profileInsertQuery = @" INSERT INTO Profile (FirstName, LastName, Email, JobTitle, isDelete, UserId, Phone)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @isDelete, @UserRef, @Phone);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var pId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
                {
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.JobTitle,
                    request.isDelete,
                    UserRef = request.UserId,
                    request.Phone
                });


                string recruitInsertQuery = @" INSERT INTO Recruiters (About, Bio, StreetAddress, Title, OrganizationRole, OrganizationApproved)
                                    VALUES (@About, @Bio, @StreetAddress, @Title, @OrganizationRole, @OrganizationApproved);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var recruitId = await _db.ExecuteScalarAsync<int>(recruitInsertQuery, new
                {
                    request.About,
                    request.Bio,
                    request.StreetAddress,
                    request.Title,
                    request.OrganizationRole,
                    request.OrganizationApproved
                });

            }
            else
            {
                string profileInsertQuery = @" INSERT INTO Recruiters (FirstName, LastName, Email, JobTitle, UserRef, Phone, About, Bio, StreetAddress, Title, OrganizationId, OrganizationRole, OrganizationApproved)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @UserRef, @Phone, @About, @Bio, @StreetAddress, @Title, @OrganizationId, @OrganizationRole, @OrganizationApproved);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                var pId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
                {
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.JobTitle,
                    UserRef = request.UserId,
                    request.Phone,
                    request.About,
                    request.Bio,
                    request.StreetAddress,
                    request.Title,
                    request.OrganizationId,
                    request.OrganizationRole,
                    request.OrganizationApproved
                });
            }


            // 5. Get updated profile (including nested data if needed)
            var updatedProfile = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
                                    SELECT 
                                        p.*, 
                                        u.Email, u.Role, u.Subscription, 
                                        o.Name as OrganizationName
                                    FROM Profile p
                                    LEFT JOIN Users u ON p.UserId = u.Id
                                    LEFT JOIN Organizations o ON p.OrganizationId = o.Id
                                    WHERE p.UserId = @UserId
                                ", new { UserId = request.UserId });

            if (updatedProfile == null)
            {
                responeModel.data = updatedProfile;
                responeModel.code = 4;
                return responeModel;
                //return (new { data = updatedProfile, code = 4 });
            }

            responeModel.data = updatedProfile;
            responeModel.code = 0;
            return responeModel;

            //return (new { data = updatedProfile, code = 0 });
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
