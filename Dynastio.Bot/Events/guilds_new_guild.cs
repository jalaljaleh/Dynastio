using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Handlers;

namespace Dynastio.Bot.Events
{
    internal class guilds_new_guild : HandlersBase
    {
        public guilds_new_guild(IServiceProvider services) : base(services)
        {
            _discord.JoinedGuild += _discord_JoinedGuild;
        }

        private async Task _discord_JoinedGuild(Discord.WebSocket.SocketGuild dGuild)
        {
            var bGuild = await _db.GetGuildAsync(dGuild.Id);

            // remove extra guilds for unverified bots..
            if (_discord.Guilds.Count > 75)
            {
                foreach (var removeG in _discord.Guilds.OrderBy(a => a.MemberCount))
                {
                    var removebGuild = await _db.GetGuildAsync(removeG.Id);
                    if (removebGuild.HasSubscription() == false)
                    {
                        await removeG.LeaveAsync();
                        break;
                    }
                }

            }

        }
    }
}
