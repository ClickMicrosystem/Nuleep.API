using Nuleep.Models;

namespace Nuleep.Data.Interface
{
    public interface IUserRepository
    {
        Task<User> GetUserAsync(string username, string password);
        Task<User> GetUserByUsernameAsync(string username);
    }
}
