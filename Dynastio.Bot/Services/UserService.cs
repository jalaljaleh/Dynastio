using Dynastio.Bot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{
    public class UserService : ServicesBase
    {
        public UserService(IServiceProvider services) : base(services)
        {

        }
        public async Task<bool> UpdateUserAsync(User user)
        {
            user.LastUpdateTime = DateTime.UtcNow;
            return await _db.UpdateAsync(user);
        }
    }
}
