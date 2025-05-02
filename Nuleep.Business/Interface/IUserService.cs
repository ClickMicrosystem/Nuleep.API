using Nuleep.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nuleep.Business.Interface
{
    public interface IUserService
    {
        Task<User> Authenticate(string username, string password);
    }
}
