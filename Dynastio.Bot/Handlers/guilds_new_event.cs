using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Handlers
{
    internal class guilds_new_event : HandlersBase
    {
        public guilds_new_event(IServiceProvider services) : base(services)
        {
            this._discord.JoinedGuild += _discord_JoinedGuild;
        }

        private async Task _discord_JoinedGuild(Discord.WebSocket.SocketGuild dGuild)
        {
            var bGuild = await this._db.GetGuildAsync(dGuild.Id);

            // remove extra guilds for unverified bots..
            while (_discord.Guilds.Count > 70)
            {
                foreach (var removeG in _discord.Guilds.OrderBy(a => a.MemberCount))
                {
                    var removebGuild = await this._db.GetGuildAsync(removeG.Id);
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
