using Nuleep.Business.Interface;
using Nuleep.Data.Interface;
using Nuleep.Models;

namespace Nuleep.Business.Services
{
    public class UserService:IUserService
    {
        
        private readonly IUserRepository _userRepo;

        public UserService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            return await _userRepo.GetUserAsync(username, password);
        }
    }
}
