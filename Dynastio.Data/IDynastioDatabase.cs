using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Dynastio.Data.RedeemCode;

namespace Dynastio.Data
{
    internal interface IDynastioDatabase
    {
        Task InitializeAsync();

        Task<Guild> GetGuildAsync(ulong Id);
        Task<bool> UpdateAsync(Guild guild);
        Task<bool> InsertAsync(Guild guild);



       internal Task<User> GetUserAsync(ulong Id);
        Task<bool> InsertAsync(User Buser);
        Task<bool> UpdateAsync(User Buser);

        Task<List<User>> GetActivityScoreLeaderboardAsync(int count = 15);
        Task<User> GetUserByAccountIdAsync(string Id);
        Task<User> GetUserByConnectedAccountIdAsync(string accountId);
        Task<User> GetUserByYoutubeChannelIdAsync(string Id);
        Task<bool> UpdateManyAsync(List<User> users);
        Task<bool> DeleteAsync(User user);

        Task<RedeemCode> GetRedeemCodeAsync(RedeemType type);
        Task<List<RedeemCode>> GetRedeemCodesAsync();
        Task<bool> InsertAsync(RedeemCode redeemCodes);
        Task<bool> InsertManyAsync(List<RedeemCode> redeemCodes);
        Task<bool> DeleteAsync(RedeemCode redeemCodes);


    }
}
