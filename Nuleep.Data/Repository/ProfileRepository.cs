using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Nuleep.Models;
using System.Data;
using Dapper;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nuleep.Data.Interface;
using Azure.Core;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Transactions;
using Nuleep.Models.Response;

namespace Nuleep.Data.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly IDbConnection _db;

        public ProfileRepository(IConfiguration config)
        {
            _db = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        }

        public async Task SaveResumeAsync(int jobSeekerId, string fileName, string blobName, string fullUrl)
        {
            var sql = @"
            UPDATE JobSeekers
            SET ResumeFileName = @FileName,
                ResumeBlobName = @BlobName,
                ResumeUrl = @Url
            WHERE Id = @Id";

            await _db.ExecuteAsync(sql, new
            {
                Id = jobSeekerId,
                FileName = fileName,
                BlobName = blobName,
                Url = fullUrl
            });
        }

        public async Task RemoveResumeReferenceAsync(int jobSeekerId)
        {
            var sql = @"
            UPDATE JobSeekers
            SET ResumeFileName = NULL,
                ResumeBlobName = NULL,
                ResumeUrl = NULL
            WHERE Id = @Id";

            await _db.ExecuteAsync(sql, new { Id = jobSeekerId });
        }

        public async Task<dynamic> ViewProfile(int profileId)
        {
            Profile profile = new Profile();
            try
            {
                var profileQuery = "SELECT UserRef FROM Profile WHERE Id = @profileId";
                var userId = await _db.QueryFirstOrDefaultAsync<int>(profileQuery, new { profileId = profileId });
                var sql = "SELECT * FROM Users WHERE Id = @UserRefId";
                var user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { UserRefId = userId });
                if (user == null)
                {
                    return profile;
                }
                if (user.Role.ToLower() == "jobseeker")
                {
                    return await GetJobSeekerByProfileId(profileId);
                }
                else
                {
                    return await GetRecruiterByProfileId(profileId);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return profile;
            }

        }


        public async Task<dynamic> GetUserByUsernameAsync(string userId)
        {
            Profile profile = new Profile();
            var user = new User();
            var profileQuery = "";
            var sql = "";
            var profileId = 0;
            try
            {
                sql = "SELECT * FROM Users WHERE Id = @UserRefId";
                user = await _db.QueryFirstOrDefaultAsync<User>(sql, new { UserRefId = int.Parse(userId) });
                profileQuery = "SELECT Id FROM Profile WHERE UserRef = @UserRefId";
                profileId = await _db.QueryFirstOrDefaultAsync<int>(profileQuery, new { UserRefId = int.Parse(userId) });
                
            }
            catch(Exception e)
            {
                Console.WriteLine("Exception Occured");
            }

            if (user == null)
            {
                return profile;
            }
            if (user.Role.ToLower() == "jobseeker")
            {
                return await GetJobSeekerByProfileId(profileId);
            }
            else
            {
                return await GetRecruiterByProfileId(profileId);
            }

        }
        
        public async Task<Profile> GetExistingProfileByUserAsync(string userId)
        {
            var sql = "SELECT * FROM Profile WHERE UserId = @UserRefId";
            return await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = userId });
        }

        public async Task<JobSeeker> GetJobSeekerProfileByUserId(string userId)
        {
            var sql = "SELECT Id FROM Profile WHERE UserId = @UserRefId";
            int profileId = await _db.QueryFirstOrDefaultAsync<int>(sql, new { UserRefId = userId });
            return await GetJobSeekerByProfileId(profileId);
        }

        public async Task<Recruiter> GetRecruiterProfileByUserId(string userId)
        {
            var sql = "SELECT Id FROM Profile WHERE UserId = @UserRefId and Type = @Type";
            int profileId = await _db.QueryFirstOrDefaultAsync<int>(sql, new { UserRefId = userId, Type = "recruiter" });
            return await GetRecruiterByProfileId(profileId);
        }

        private dynamic GetCreatedProfileData(string profileId)
        {
            var query = @"SELECT Id, FirstName, LastName, FullName, JobTitle, Email FROM Profile WHERE UserId = @UserId;
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

        public async Task<JobSeeker> CreateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest)
        {

            JobSeeker jobSeeker = new JobSeeker();

            var sql = "SELECT Id, FirstName, LastName, Email, FullName, JobTitle, Phone FROM Profile WHERE UserRef = @UserRefId";
            var existingProfile = await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = profileRequest.UserId });

            string insertQuery = "";
            int profileId = 0;
            int JobSeekersId = 0;
            int CareerJourneyId = 0;
            int MyStoryId = 0;

            if (existingProfile == null)
            {
                insertQuery = @" INSERT INTO Profile (FirstName, LastName, Email, JobTitle, UserRef, Phone)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @UserRef, @Phone);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                profileId = await _db.ExecuteScalarAsync<int>(insertQuery, new
                {
                    profileRequest.FirstName,
                    profileRequest.LastName,
                    profileRequest.Email,
                    profileRequest.JobTitle,
                    UserRef = profileRequest.UserId,
                    profileRequest.Phone
                });

                insertQuery = @"INSERT INTO JobSeekers (ProfileId, HeaderImageFileName, HeaderImageBlobName, HeaderImageFullUrl, Bio, CurrentCompany, Remote, WebsiteUrl, CareerPath, StreetAddress, CountryRegion, City, StateProvince, ZipPostal, Skills, classes) VALUES (@ProfileId, @HeaderImageFileName, @HeaderImageBlobName, @HeaderImageFullUrl, @Bio, CurrentCompany, @Remote, WebsiteUrl, @CareerPath, @StreetAddress, @CountryRegion, @City, @StateProvince, @ZipPostal, @Skills, @classes);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                JobSeekersId = await _db.ExecuteScalarAsync<int>(insertQuery, new
                {
                    profileId,
                    HeaderImageFileName = profileRequest.HeaderImage.FileName,
                    HeaderImageBlobName = profileRequest.HeaderImage.BlobName,
                    HeaderImageFullUrl = profileRequest.HeaderImage.FullUrl,
                    profileRequest.Bio,
                    profileRequest.CurrentCompany,
                    profileRequest.Remote,
                    profileRequest.WebsiteUrl,
                    profileRequest.CareerPath,
                    profileRequest.StreetAddress,
                    profileRequest.CountryRegion,
                    profileRequest.City,
                    profileRequest.StateProvince,
                    profileRequest.ZipPostal,
                    Skills = System.String.Join(", ", profileRequest.Skills),
                    Classes = System.String.Join(", ", profileRequest.Classes)
                });
            }
            else
            {
                profileId = existingProfile.Id;
                if (profileRequest.FirstName != null && profileRequest.StreetAddress != null)
                {
                    string updateProfileQuery = @"
                    UPDATE Profile
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        JobTitle = @JobTitle
                    WHERE Id = @ProfileId;";



                    string updateJobSeekersQuery = @"
                    UPDATE JobSeekers
                    SET Bio = @Bio,
                        StreetAddress = @StreetAddress
                    WHERE ProfileId = @ProfileId;";

                    try
                    {
                        await _db.ExecuteAsync(updateProfileQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            profileRequest.FirstName,
                            profileRequest.LastName,
                            profileRequest.JobTitle,
                        });

                        await _db.ExecuteAsync(updateJobSeekersQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            profileRequest.Bio,
                            profileRequest.StreetAddress
                        });
                    }
                    catch (Exception e)
                    {

                    }
                }
            }

            if (profileRequest.Education?.Count() > 0)
            {
                foreach (Education educationItem in profileRequest.Education)
                {
                    string EducationInsertQuery = @"INSERT INTO Education (ProfileId, SchoolOrOrganization, DegreeCertification, FieldOfStudy, [From], [To], Present, Description)
                                    VALUES (@ProfileId, @SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @Description);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var EducationId = await _db.ExecuteScalarAsync<int>(EducationInsertQuery, new
                    {
                        profileId,
                        educationItem.SchoolOrOrganization,
                        educationItem.DegreeCertification,
                        educationItem.FieldOfStudy,
                        educationItem.From,
                        educationItem.To,
                        educationItem.Present,
                        educationItem.Description
                    });
                }
            }

            if (profileRequest.Awards?.Count() > 0)
            {
                foreach (Award award in profileRequest.Awards)
                {
                    string AwardsInsertQuery = @"INSERT INTO Awards (ProfileId, AwardName, CompanyName, [Date], [Description])
                                    VALUES (@ProfileId, @AwardName, @CompanyName, @Date, @Description);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var AwardId = await _db.ExecuteScalarAsync<int>(AwardsInsertQuery, new
                    {
                        profileId,
                        award.AwardName,
                        award.CompanyName,
                        award.Date,
                        award.Description
                    });
                }
            }

            if (profileRequest.Experience?.Count() > 0)
            {
                foreach (Experience experience in profileRequest.Experience)
                {
                    string ExperienceInsertQuery = @"INSERT INTO Experience (ProfileId, Title, Company, [Location], [DescriptionC], FromDate, ToDate, [Current], Impact, [Description])
                                    VALUES (@ProfileId, @Title, @Company, @Location, @DescriptionC, @FromDate, @ToDate, @Current, @Impact, @Description);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var AwardId = await _db.ExecuteScalarAsync<int>(ExperienceInsertQuery, new
                    {
                        profileId,
                        experience.Title,
                        experience.Company,
                        experience.Location,
                        experience.DescriptionC,
                        experience.From,
                        experience.To,
                        experience.Current,
                        Impact = System.String.Join(", ", experience.Impact),
                        Description = System.String.Join(", ", experience.Description)
                    });
                }
            }

            if (profileRequest.References?.Count() > 0)
            {
                foreach (Reference reference in profileRequest.References)
                {
                    string ReferencesInsertQuery = @"INSERT INTO [References] (ProfileId, Name, Company, [Email], [Phone])
                                    VALUES (@ProfileId, @Name, @Email, @Phone);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var ReferenceId = await _db.ExecuteScalarAsync<int>(ReferencesInsertQuery, new
                    {
                        profileId,
                        reference.Name,
                        reference.Company,
                        reference.Email,
                        reference.Phone
                    });
                }
            }

            if (profileRequest.CareerJourney != null)
            {
                string CareerJourneyInsertQuery = @"INSERT INTO CareerJourney (JobSeekerId, NextRole, Description, Experience, Training)
                                    VALUES (@JobSeekerId, @NextRole, @Description, @Experience, @Training);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                CareerJourneyId = await _db.ExecuteScalarAsync<int>(CareerJourneyInsertQuery, new
                {
                    JobSeekersId,
                    profileRequest.CareerJourney.NextRole,
                    profileRequest.CareerJourney.Description,
                    Experience = System.String.Join(", ", profileRequest.CareerJourney.Experience),
                    Training = System.String.Join(", ", profileRequest.CareerJourney.Training)
                });
            }

            if (profileRequest.MyStory != null)
            {
                string MyStoryInsertQuery = @"INSERT INTO MyStory (ProfileId, Header, Summary)
                                    VALUES (@ProfileId, @Header, @Summary);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                MyStoryId = await _db.ExecuteScalarAsync<int>(MyStoryInsertQuery, new
                {
                    profileId,
                    profileRequest.MyStory.Header,
                    profileRequest.MyStory.Summary
                });

                foreach(Activity activity in profileRequest.MyStory.Activities)
                {
                    string ActivityInsertQuery = @"INSERT INTO MyStoryActivities (MyStoryId, Title, ImageBlobName, ImageFullUrl, Skills)
                                    VALUES (@MyStoryId, @Title, @ImageBlobName, @ImageFullUrl, @Skills);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                    var actvityId = await _db.ExecuteScalarAsync<int>(ActivityInsertQuery, new
                    {
                        MyStoryId,
                        activity.Title,
                        ImageBlobName = activity.Image.BlobName,
                        ImageFullUrl = activity.Image.FullUrl,
                        Skills = System.String.Join(", ", activity.Skills)
                    });
                }
            }
            return await GetJobSeekerByProfileId(profileId);
        }

        public async Task<Recruiter> CreateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest)
        { 
            Recruiter recruiter = new Recruiter();
            var sql = "SELECT Id, FirstName, LastName, Email, FullName, JobTitle, Phone FROM Profile WHERE UserRef = @UserRefId";
            var existingProfile = await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = profileRequest.UserId });

            string insertQuery = "";
            int profileId = 0;
            int RecruiterId = 0;

            if (existingProfile == null)
            {
                insertQuery = @" INSERT INTO Profile (FirstName, LastName, Email, JobTitle, UserRef, Phone)
                                    VALUES (@FirstName, @LastName, @Email, @JobTitle, @UserRef, @Phone);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                profileId = await _db.ExecuteScalarAsync<int>(insertQuery, new
                {
                    profileRequest.FirstName,
                    profileRequest.LastName,
                    profileRequest.Email,
                    profileRequest.JobTitle,
                    UserRef = profileRequest.UserId,
                    profileRequest.Phone
                });

                insertQuery = @"INSERT INTO Recruiters (ProfileId, About, Bio, StreetAddress, Title, OrganizationRole, OrganizationApproved)
                                    VALUES (@ProfileId, @About, @Bio, @StreetAddress, @Title, @OrganizationRole, @OrganizationApproved);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                RecruiterId = await _db.ExecuteScalarAsync<int>(insertQuery, new
                {
                    profileId,
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
                if(profileRequest.FirstName != null && profileRequest.StreetAddress != null)
                {
                    string updateProfileQuery = @"
                    UPDATE Profile
                    SET FirstName = @FirstName,
                        LastName = @LastName,
                        JobTitle = @JobTitle
                    WHERE Id = @ProfileId;";



                    string updateRecruiterQuery = @"
                    UPDATE Recruiters
                    SET Bio = @Bio,
                        StreetAddress = @StreetAddress
                    WHERE ProfileId = @ProfileId;";

                    try
                    {
                        await _db.ExecuteAsync(updateProfileQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            profileRequest.FirstName,
                            profileRequest.LastName,
                            profileRequest.JobTitle,
                        });

                        await _db.ExecuteAsync(updateRecruiterQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            profileRequest.Bio,
                            profileRequest.StreetAddress
                        });
                    }
                    catch (Exception e)
                    {

                    }
                }                
            }

            if (profileRequest.Education?.Count() > 0)
            {
                foreach (Education educationItem in profileRequest.Education)
                {
                    if (educationItem.Id == 0 || educationItem.Id == null)
                    {
                        string EducationInsertQuery = @"INSERT INTO Education (ProfileId, SchoolOrOrganization, DegreeCertification, FieldOfStudy, [From], [To], Present, Description)
                                    VALUES (@ProfileId, @SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @Description);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";

                        var EducationId = await _db.ExecuteScalarAsync<int>(EducationInsertQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            educationItem.SchoolOrOrganization,
                            educationItem.DegreeCertification,
                            educationItem.FieldOfStudy,
                            educationItem.From,
                            educationItem.To,
                            educationItem.Present,
                            educationItem.Description
                        });
                    }
                    else
                    {
                        string EducationUpdateQuery = @"
                        UPDATE Education
                        SET 
                            SchoolOrOrganization = @SchoolOrOrganization,
                            DegreeCertification = @DegreeCertification,
                            FieldOfStudy = @FieldOfStudy,
                            [From] = @From,
                            [To] = @To,
                            Present = @Present,
                            Description = @Description
                        WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(EducationUpdateQuery, new
                        {
                            Id = educationItem.Id,
                            ProfileId = existingProfile.Id,
                            educationItem.SchoolOrOrganization,
                            educationItem.DegreeCertification,
                            educationItem.FieldOfStudy,
                            educationItem.From,
                            educationItem.To,
                            educationItem.Present,
                            educationItem.Description
                        });
                    }
                }
            }

            if (profileRequest.Awards?.Count() > 0)
            {
                foreach (Award award in profileRequest.Awards)
                {
                    if(award.Id == 0)
                    {
                        string AwardsInsertQuery = @"INSERT INTO Awards (ProfileId, AwardName, CompanyName, [Date], [Description])
                                    VALUES (@ProfileId, @AwardName, @CompanyName, @Date, @Description);
                                    SELECT CAST(SCOPE_IDENTITY() as int);";
                        var AwardId = await _db.ExecuteScalarAsync<int>(AwardsInsertQuery, new
                        {
                            ProfileId = existingProfile.Id,
                            award.AwardName,
                            award.CompanyName,
                            award.Date,
                            award.Description
                        });
                    }
                    else
                    {
                        string AwardsUpdateQuery = @"
                        UPDATE Awards
                        SET 
                            AwardName = @AwardName,
                            CompanyName = @CompanyName,
                            [Date] = @Date,
                            [Description] = @Description
                        WHERE Id = @Id AND ProfileId = @ProfileId;";

                        await _db.ExecuteAsync(AwardsUpdateQuery, new
                        {
                            Id = award.Id,
                            ProfileId = existingProfile.Id,
                            award.AwardName,
                            award.CompanyName,
                            award.Date,
                            award.Description
                        });
                    }

                }
            }
            
            return await GetRecruiterByProfileId(existingProfile.Id);
        
        }

        public async Task<JobSeeker> UpdateJobSeekerProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            var sql = "SELECT Id FROM Profile WHERE UserRef = @UserRefId";
            var existingProfile = await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = profileRequest.UserId });
            if (existingProfile == null)
                throw new Exception("Profile not found");

            int profileId = existingProfile.Id;

            string updateProfileSql = @"UPDATE Profile SET FirstName = @FirstName, LastName = @LastName, Email = @Email, JobTitle = @JobTitle, Phone = @Phone 
                                WHERE Id = @ProfileId";
            await _db.ExecuteAsync(updateProfileSql, new
            {
                profileRequest.FirstName,
                profileRequest.LastName,
                profileRequest.Email,
                profileRequest.JobTitle,
                profileRequest.Phone,
                ProfileId = profileId
            });

            string updateJobSeekerSql = @"UPDATE JobSeekers SET HeaderImageFileName = @HeaderImageFileName, HeaderImageBlobName = @HeaderImageBlobName, 
                                  HeaderImageFullUrl = @HeaderImageFullUrl, Bio = @Bio, CurrentCompany = @CurrentCompany, Remote = @Remote, 
                                  WebsiteUrl = @WebsiteUrl, CareerPath = @CareerPath, StreetAddress = @StreetAddress, CountryRegion = @CountryRegion,
                                  City = @City, StateProvince = @StateProvince, ZipPostal = @ZipPostal, Skills = @Skills, classes = @Classes
                                  WHERE ProfileId = @ProfileId";
            await _db.ExecuteAsync(updateJobSeekerSql, new
            {
                profileRequest.HeaderImage.FileName,
                profileRequest.HeaderImage.BlobName,
                profileRequest.HeaderImage.FullUrl,
                profileRequest.Bio,
                profileRequest.CurrentCompany,
                profileRequest.Remote,
                profileRequest.WebsiteUrl,
                profileRequest.CareerPath,
                profileRequest.StreetAddress,
                profileRequest.CountryRegion,
                profileRequest.City,
                profileRequest.StateProvince,
                profileRequest.ZipPostal,
                Skills = string.Join(", ", profileRequest.Skills),
                Classes = string.Join(", ", profileRequest.Classes),
                ProfileId = profileId
            });

            // Optional: delete existing education, awards, experience, references, etc., and reinsert
            await _db.ExecuteAsync("DELETE FROM Education WHERE ProfileId = @ProfileId", new { ProfileId = profileId });
            foreach (var edu in profileRequest.Education ?? Enumerable.Empty<Education>())
            {
                string sqlInsertEdu = @"INSERT INTO Education (ProfileId, SchoolOrOrganization, DegreeCertification, FieldOfStudy, [From], [To], Present, Description)
                                VALUES (@ProfileId, @SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @Description)";
                await _db.ExecuteAsync(sqlInsertEdu, new
                {
                    ProfileId = profileId,
                    edu.SchoolOrOrganization,
                    edu.DegreeCertification,
                    edu.FieldOfStudy,
                    edu.From,
                    edu.To,
                    edu.Present,
                    edu.Description
                });
            }

            // Similarly update Awards, Experience, References, CareerJourney, MyStory...

            // Return updated job seeker using the existing query
            return await GetJobSeekerByProfileId(profileId);
        }

        public async Task<Recruiter> UpdateRecruiterProfile(CreateOrUpdateProfileRequest profileRequest)
        {
            var sql = "SELECT Id FROM Profile WHERE UserRef = @UserRefId";
            var existingProfile = await _db.QueryFirstOrDefaultAsync<Profile>(sql, new { UserRefId = profileRequest.UserId });
            if (existingProfile == null)
                throw new Exception("Profile not found");

            int profileId = existingProfile.Id;

            string updateProfileSql = @"UPDATE Profile SET FirstName = @FirstName, LastName = @LastName, Email = @Email, JobTitle = @JobTitle, Phone = @Phone 
                                WHERE Id = @ProfileId";
            await _db.ExecuteAsync(updateProfileSql, new
            {
                profileRequest.FirstName,
                profileRequest.LastName,
                profileRequest.Email,
                profileRequest.JobTitle,
                profileRequest.Phone,
                ProfileId = profileId
            });

            string updateRecruiterSql = @"UPDATE Recruiters SET About = @About, Bio = @Bio, StreetAddress = @StreetAddress, Title = @Title,
                                  OrganizationRole = @OrganizationRole, OrganizationApproved = @OrganizationApproved
                                  WHERE ProfileId = @ProfileId";
            await _db.ExecuteAsync(updateRecruiterSql, new
            {
                profileRequest.About,
                profileRequest.Bio,
                profileRequest.StreetAddress,
                profileRequest.Title,
                profileRequest.OrganizationRole,
                profileRequest.OrganizationApproved,
                ProfileId = profileId
            });

            await _db.ExecuteAsync("DELETE FROM Education WHERE ProfileId = @ProfileId", new { ProfileId = profileId });
            foreach (var edu in profileRequest.Education ?? Enumerable.Empty<Education>())
            {
                string sqlInsertEdu = @"INSERT INTO Education (ProfileId, SchoolOrOrganization, DegreeCertification, FieldOfStudy, [From], [To], Present, Description)
                                VALUES (@ProfileId, @SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @Description)";
                await _db.ExecuteAsync(sqlInsertEdu, new
                {
                    ProfileId = profileId,
                    edu.SchoolOrOrganization,
                    edu.DegreeCertification,
                    edu.FieldOfStudy,
                    edu.From,
                    edu.To,
                    edu.Present,
                    edu.Description
                });
            }

            return await GetRecruiterByProfileId(profileId);
        }

        private async Task<JobSeeker> GetJobSeekerByProfileId(int profileId)
        {
            try
            {

                string sql = @"
                            SELECT * FROM Profile WHERE Id = @ProfileId;
                            SELECT * FROM JobSeekers WHERE ProfileId = @ProfileId;
                            SELECT * FROM Users WHERE Id = (SELECT UserRef FROM Profile WHERE Id = @ProfileId);
                            SELECT * FROM Subscriptions WHERE UserId = (SELECT UserRef FROM Profile WHERE Id = @ProfileId);
                            SELECT Id, Email, Name FROM Organizations WHERE ProfileId = @ProfileId;
                            SELECT * FROM Education WHERE ProfileId = @ProfileId;
                            SELECT * FROM Awards WHERE ProfileId = @ProfileId;
                            SELECT * FROM Experience WHERE ProfileId = @ProfileId;
                            SELECT * FROM [References] WHERE ProfileId = @ProfileId;
                            SELECT * FROM CareerJourney WHERE JobSeekerId = (Select Id from JobSeekers where ProfileId = @ProfileId);
                            SELECT * FROM MyStory WHERE ProfileId = @ProfileId;
                            SELECT * FROM ProfileImages WHERE ProfileId = @ProfileId Order By Id Desc;
                            SELECT * FROM ProjectImages WHERE ProfileId = @ProfileId;
                            SELECT * FROM HeaderImages where [JobSeekerId] = (Select Id from JobSeekers WHERE ProfileId = @ProfileId);
                            Select cje.Experience from CareerJourneyExperience cje inner join CareerJourney cj on cje.CareerJourneyId = cj.Id
                            inner join JobSeekers js on cj.JobSeekerId = js.Id where js.ProfileId = @ProfileId;
                            Select cjt.Training from CareerJourneyTraining cjt inner join CareerJourney cj on cjt.CareerJourneyId = cj.Id
                            inner join JobSeekers js on cj.JobSeekerId = js.Id where js.ProfileId = @ProfileId;
                            ";

                //SELECT* FROM Interests WHERE ProfileId = @ProfileId;
                //SELECT* FROM Resumes WHERE ProfileId = @ProfileId;



                using var multi = await _db.QueryMultipleAsync(sql, new { ProfileId = profileId });

                var profile = await multi.ReadFirstOrDefaultAsync<Profile>();
                var jobSeekerData = await multi.ReadFirstOrDefaultAsync<JobSeeker>();
                var user = await multi.ReadFirstOrDefaultAsync<User>();
                var subscription = await multi.ReadFirstOrDefaultAsync<Subscription>();
                var organization = await multi.ReadFirstOrDefaultAsync<Organization>();
                var education = (await multi.ReadAsync<Education>()).ToList();
                var awards = (await multi.ReadAsync<Award>()).ToList();
                var experience = (await multi.ReadAsync<Experience>()).ToList();
                var references = (await multi.ReadAsync<Reference>()).ToList();
                var careerJourney = await multi.ReadAsync<CareerJourney>();
                var myStory = await multi.ReadAsync<MyStory>();
                var profileImg = (await multi.ReadAsync<MediaImage>()).ToList();
                var projectImage = (await multi.ReadAsync<MediaImage>()).ToList();
                var headerImage = await multi.ReadFirstOrDefaultAsync<MediaImage>();
                var careerJourneyExperence = (await multi.ReadAsync<string>()).ToList();
                var careerJourneyTraining = (await multi.ReadAsync<string>()).ToList();
                //var interests = (await multi.ReadAsync<>()).ToList();
                //var resumes = (await multi.ReadAsync<Resum>()).ToList();
                //var projectImages = (await multi.ReadAsync<Project>()).ToList();

                return new JobSeeker
            {
                HeaderImage = new MediaImage() { BlobName = headerImage?.BlobName, FileName = headerImage?.FileName, FullUrl = headerImage?.FullUrl },
                CareerJourney = new CareerJourney() { Experience = careerJourneyExperence, Training = careerJourneyTraining },
                MyStory = new MyStory() { Activities = [] },
                Remote = jobSeekerData?.Remote,
                SkillList = jobSeekerData?.Skills.Split(',').ToList() ?? new List<string>(),
                ClassList = jobSeekerData?.Classes.Split(',').ToList() ?? new List<string>(),
                SavedJobs = [],
                RecentlyViewJobs = [],
                IsDelete = jobSeekerData?.IsDelete,
                ChatRooms = [],
                Awards = awards,
                ProjectImg = projectImage,
                RecentlyViwedCourses = [],
                SavedCourses = [],
                Education = education,
                Experience = experience,
                References = references,
                Interests = [],
                Resume = [],
                CreatedAt = jobSeekerData.CreatedAt,
                ProfileImage = profileImg,
                Id = profile.Id,
                User = new User
                {
                    Id = user.Id,
                    Email = user.Email,
                    Role = user.Role
                },
                Email = profile.Email,
                LastName = profile.LastName,
                FirstName = profile.FirstName,
                JobTitle = profile.JobTitle,
                StreetAddress = jobSeekerData.StreetAddress,
                Type = profile.Type,
                JobSeekerId = jobSeekerData.JobSeekerId
                };

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

            return new JobSeeker();
        }

        private async Task<Recruiter> GetRecruiterByProfileId(int profileId)
        {
            try
            {
                string sql = @"
                                SELECT * FROM Profile WHERE Id = @ProfileId;
                                SELECT * FROM Recruiters WHERE ProfileId = @ProfileId;
                                SELECT * FROM Users WHERE Id = (SELECT UserRef FROM Profile WHERE Id = @ProfileId);
                                SELECT * FROM Subscriptions WHERE UserId = (SELECT UserRef FROM Profile WHERE Id = @ProfileId);
                                SELECT Id, Email, Name FROM Organizations WHERE ProfileId = @ProfileId;
                                SELECT * FROM Education WHERE ProfileId = @ProfileId;
                                SELECT * FROM Awards WHERE ProfileId = @ProfileId;
                                SELECT * FROM ProfileImages WHERE ProfileId = @ProfileId Order by Id Desc;
                                ";

                using var multi = await _db.QueryMultipleAsync(sql, new { ProfileId = profileId });

                var profile = await multi.ReadFirstOrDefaultAsync<Profile>();
                var recruiterData = await multi.ReadFirstOrDefaultAsync<Recruiter>();
                var user = await multi.ReadFirstOrDefaultAsync<User>();
                var subscription = await multi.ReadFirstOrDefaultAsync<Subscription>();
                var organization = await multi.ReadFirstOrDefaultAsync<Organization>();
                var education = (await multi.ReadAsync<Education>()).ToList();
                var awards = (await multi.ReadAsync<Award>()).ToList();
                var profileImg = (await multi.ReadAsync<MediaImage>()).ToList();

                return new Recruiter
                {
                    Id = profile.Id,
                    User = new User
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        Role = user.Role,
                        subscription = subscription
                    },
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Email = profile.Email,
                    JobTitle = profile.JobTitle,
                    Phone = profile.Phone,
                    About = recruiterData.About,
                    Bio = recruiterData.Bio,
                    Type=profile.Type,
                    StreetAddress = recruiterData.StreetAddress,
                    Title = recruiterData.Title,
                    OrganizationRole = recruiterData.OrganizationRole,
                    OrganizationApproved = recruiterData.OrganizationApproved,
                    Organization = organization,
                    Education = education,
                    Awards = awards,
                    ProfileImage = profileImg,
                    OrganizationId = recruiterData?.OrganizationId
                };

            }
            catch (Exception ex) {
                Console.WriteLine(ex);
                return new Recruiter();
            }
        }

        public async Task<ProfileResponse> UpdateProfile(CreateOrUpdateProfileRequest request)
        {
            ProfileResponse responeModel = new ProfileResponse();

            await Task.Delay(100);

            #region Reference Code 
            //// 1. Find existing profile
            //var profileId = await _db.QueryFirstOrDefaultAsync<int>(
            //                "SELECT Id FROM Profile WHERE UserId = @UserId",
            //                new { UserId = request.UserId }
            //            );

            //if (profileId < 1)
            //{
            //    responeModel.data = profileId;
            //    responeModel.code = 1;
            //    return responeModel;
            //    //return (new { data = profileId, code = 1 });
            //}

            //// 3. Validate Role if Provided
            //if(request.Role.ToLower() != "recruiter" && request.Role.ToLower() != "jobSeeker")
            //{
            //    responeModel.data = profileId;
            //    responeModel.code = 3;
            //    return responeModel;
            //    //return (new { data = profileId, code = 3 });
            //}


            //if (request.Education.Count > 0)
            //{
            //    foreach (var item in request.Education)
            //    {
            //        if (item.Id == 0)
            //        {
            //            string educationInsertQuery = @" INSERT INTO Education (SchoolOrOrganization, DegreeCertification, FieldOfStudy, From, To, Present, ProfileId)
            //                        VALUES (@SchoolOrOrganization, @DegreeCertification, @FieldOfStudy, @From, @To, @Present, @ProfileId);
            //                        SELECT CAST(SCOPE_IDENTITY() as int);";

            //            var educationId = await _db.ExecuteScalarAsync<int>(educationInsertQuery, new
            //            {
            //                item.SchoolOrOrganization,
            //                item.DegreeCertification,
            //                item.FieldOfStudy,
            //                item.From,
            //                item.To,
            //                item.Present,
            //                ProfileId = profileId
            //            });
            //        }
            //        else
            //        {
            //            string educationUpdateQuery = @" UPDATE Education
            //                                                SET
            //                                                    SchoolOrOrganization = @SchoolOrOrganization,
            //                                                    DegreeCertification = @DegreeCertification,
            //                                                    FieldOfStudy = @FieldOfStudy,
            //                                                    [From] = @From,
            //                                                    [To] = @To,
            //                                                    Present = @Present
            //                                                WHERE Id = @Id AND ProfileId = @ProfileId;";

            //            await _db.ExecuteAsync(educationUpdateQuery, new
            //            {
            //                item.SchoolOrOrganization,
            //                item.DegreeCertification,
            //                item.FieldOfStudy,
            //                item.From,
            //                item.To,
            //                item.Present,
            //                item.Id,
            //                ProfileId = profileId
            //            });


            //        }
            //    }
            //}

            //if (request.Awards.Count > 0)
            //{
            //    foreach (var item in request.Awards)
            //    {
            //        if (item.Id == 0)
            //        {
            //            string awardInsertQuery = @" INSERT INTO Awards (AwardName, CompanyName, Date, Description, ProfileId)
            //                        VALUES (@SchoolOrOrganization, @CompanyName, @Date, @Description, @ProfileId);
            //                        SELECT CAST(SCOPE_IDENTITY() as int);";

            //            var educationId = await _db.ExecuteScalarAsync<int>(awardInsertQuery, new
            //            {
            //                item.AwardName,
            //                item.CompanyName,
            //                item.Date,
            //                item.Description,
            //                item.ProfileId
            //            });
            //        }
            //        else
            //        {
            //            string educationUpdateQuery = @" UPDATE Awards
            //                                                SET
            //                                                    AwardName = @AwardName,
            //                                                    CompanyName = @CompanyName,
            //                                                    Date = @Date,
            //                                                    Description = @Description
            //                                                WHERE Id = @Id AND ProfileId = @ProfileId;";

            //            await _db.ExecuteAsync(educationUpdateQuery, new
            //            {
            //                item.AwardName,
            //                item.CompanyName,
            //                item.Date,
            //                item.Description,
            //                item.Id,
            //                item.ProfileId
            //            });


            //        }
            //    }
            //}

            //if (profileId > 0)
            //{
            //    responeModel.data = profileId;
            //    responeModel.code = 1;
            //    return responeModel;
            //    //return new { data = profileId, code = 1 };
            //}

            //if (request.Role?.ToLower() == "recruiter")
            //{
            //    string profileInsertQuery = @" INSERT INTO Profile (FirstName, LastName, Email, JobTitle, isDelete, UserId, Phone)
            //                        VALUES (@FirstName, @LastName, @Email, @JobTitle, @isDelete, @UserRef, @Phone);
            //                        SELECT CAST(SCOPE_IDENTITY() as int);";

            //    var pId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
            //    {
            //        request.FirstName,
            //        request.LastName,
            //        request.Email,
            //        request.JobTitle,
            //        request.isDelete,
            //        UserRef = request.UserId,
            //        request.Phone
            //    });


            //    string recruitInsertQuery = @" INSERT INTO Recruiters (About, Bio, StreetAddress, Title, OrganizationRole, OrganizationApproved)
            //                        VALUES (@About, @Bio, @StreetAddress, @Title, @OrganizationRole, @OrganizationApproved);
            //                        SELECT CAST(SCOPE_IDENTITY() as int);";

            //    var recruitId = await _db.ExecuteScalarAsync<int>(recruitInsertQuery, new
            //    {
            //        request.About,
            //        request.Bio,
            //        request.StreetAddress,
            //        request.Title,
            //        request.OrganizationRole,
            //        request.OrganizationApproved
            //    });

            //}
            //else
            //{
            //    string profileInsertQuery = @" INSERT INTO Recruiters (FirstName, LastName, Email, JobTitle, UserRef, Phone, About, Bio, StreetAddress, Title, OrganizationId, OrganizationRole, OrganizationApproved)
            //                        VALUES (@FirstName, @LastName, @Email, @JobTitle, @UserRef, @Phone, @About, @Bio, @StreetAddress, @Title, @OrganizationId, @OrganizationRole, @OrganizationApproved);
            //                        SELECT CAST(SCOPE_IDENTITY() as int);";

            //    var pId = await _db.ExecuteScalarAsync<int>(profileInsertQuery, new
            //    {
            //        request.FirstName,
            //        request.LastName,
            //        request.Email,
            //        request.JobTitle,
            //        UserRef = request.UserId,
            //        request.Phone,
            //        request.About,
            //        request.Bio,
            //        request.StreetAddress,
            //        request.Title,
            //        request.OrganizationId,
            //        request.OrganizationRole,
            //        request.OrganizationApproved
            //    });
            //}


            //// 5. Get updated profile (including nested data if needed)
            //var updatedProfile = await _db.QueryFirstOrDefaultAsync<dynamic>(@"
            //                        SELECT 
            //                            p.*, 
            //                            u.Email, u.Role, u.Subscription, 
            //                            o.Name as OrganizationName
            //                        FROM Profile p
            //                        LEFT JOIN Users u ON p.UserId = u.Id
            //                        LEFT JOIN Organizations o ON p.OrganizationId = o.Id
            //                        WHERE p.UserId = @UserId
            //                    ", new { UserId = request.UserId });

            //if (updatedProfile == null)
            //{
            //    responeModel.data = updatedProfile;
            //    responeModel.code = 4;
            //    return responeModel;
            //    //return (new { data = updatedProfile, code = 4 });
            //}

            //responeModel.data = updatedProfile;
            //responeModel.code = 0;
            //return responeModel;

            //return (new { data = updatedProfile, code = 0 });

            #endregion

            return responeModel;
        }

        public async Task<JobSeeker> DeleteResumeAsync(MediaImage mediaImage)
        {
            await _db.ExecuteAsync(
                "DELETE FROM Resumes WHERE JobSeekerId = @JobSeekerId",
                new { JobSeekerId = mediaImage.ProfileId });

            return await GetJobSeekerByProfileId(mediaImage.ProfileId??0);
        }

        public async Task<JobSeeker> UpdateResumeAsync(int pId, MediaImage mediaImage)
        {
            await _db.ExecuteAsync(
                "DELETE FROM Resumes WHERE JobSeekerId = @JobSeekerId",
                new { JobSeekerId = mediaImage.ProfileId });

            return await GetJobSeekerByProfileId(pId);
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
        
        public async Task<dynamic> UpdateProfileImage(int profileId, MediaImage mediaImage)
        {
            ResponeModel responeModel = new ResponeModel();
            string insertQuery = @" INSERT INTO ProfileImages (ProfileId, FileName, BlobName, FullUrl)
                    VALUES (@ProfileId, @FileName, @BlobName, @FullUrl);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            int profileImageId = await _db.ExecuteScalarAsync<int>(insertQuery, new
            {
                ProfileId = profileId,
                FileName = mediaImage.FileName,
                BlobName = mediaImage.BlobName,
                FullUrl = mediaImage.FullUrl,
            });


            if(profileImageId > 0)
            {
                responeModel.data = ViewProfile(profileId);
            }
            return responeModel;
        }
        public async Task<dynamic> UpdateHeaderImage(int profileId, MediaImage mediaImage)
        {
            ResponeModel responeModel = new ResponeModel();

            int jobSeekerId = await _db.QueryFirstOrDefaultAsync<int>(
            "Select Id from JobSeekers Where ProfileId = @ProfileId", new { profileId });

            string insertQuery = @" INSERT INTO HeaderImages (JobSeekerId, FileName, BlobName, FullUrl)
                    VALUES (@JobSeekerId, @FileName, @BlobName, @FullUrl);
                    SELECT CAST(SCOPE_IDENTITY() as int);";

            


            if(jobSeekerId > 0)
            {
                try
                {

                    int profileImageId = await _db.ExecuteScalarAsync<int>(insertQuery, new
                    {
                        JobSeekerId = jobSeekerId,
                        FileName = mediaImage.FileName,
                        BlobName = mediaImage.BlobName,
                        FullUrl = mediaImage.FullUrl,
                    });
                    responeModel.data = ViewProfile(profileId);
                }
                catch(Exception e)
                {

                }
            }
            return responeModel;
        }

        public async Task<Recruiter?> GetAdminRecruiterProfileByOrgId(int orgId)
        {

            var sql = "SELECT ProfileId FROM Recruiters WHERE OrganizationId = @OrgId and OrganizationRole = 'admin'";
            int profileId = await _db.QueryFirstOrDefaultAsync<int>(sql, new { OrgId = orgId });
            return await GetRecruiterByProfileId(profileId);
        }

    }
}
