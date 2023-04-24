using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot;
using Discord.WebSocket;
using Discord;
using Dynastio;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions
{
    public class RequireGuild : PreconditionAttribute
    {
        public ulong? GuildId
        {
            get;
        }
        public LocalGuildId? LocalGuild
        {
            get;
        }
        public enum LocalGuildId
        {
            Main
        }
        public RequireGuild(ulong GuildId)
        {
            this.GuildId = GuildId;
        }
        public RequireGuild(LocalGuildId guild)
        {
            this.LocalGuild = guild;
        }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (Program.IsDebug()) return Task.FromResult(PreconditionResult.FromSuccess());

            if (LocalGuild.HasValue ? context.Guild.Id == services.GetRequiredService<Configuration>().Guilds.MainServer : context.Guild.Id == GuildId)
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
            {
                return Task.FromResult(PreconditionResult.FromError("This Guild Is Not Supported."));
            }
        }
    }
}
