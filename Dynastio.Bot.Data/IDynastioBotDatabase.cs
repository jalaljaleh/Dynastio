using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Data
{
    public interface IDynastioBotDatabase
    {
        Task InitializeAsync();

        Task<Guild> GetGuildAsync(Expression<Func<Guild, bool>> expression);
        Task<bool> UpdateAsync(Guild guild);
        Task<bool> InsertAsync(Guild guild);



        Task<User> GetUserAsync(Expression<Func<User, bool>> expression);
        Task<bool> InsertAsync(User Buser);
        Task<bool> UpdateAsync(User Buser);

        Task<List<User>> GetHonorLeaderboardAsync(int count = 10);
        Task<User> GetUserByAccountIdAsync(string Id);
        Task<bool> UpdateManyAsync(List<User> users);
        Task<bool> DeleteAsync(User user);
    }
}
