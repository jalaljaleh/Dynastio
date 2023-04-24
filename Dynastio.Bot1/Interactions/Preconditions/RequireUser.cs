using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Dynastio;
using Dynastio.Bot;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Interactions
{
    public class RequireUserAttribute : PreconditionAttribute
    {
        public RequireUserAttribute(RequireUserType requireUserType = RequireUserType.FromMention)
        {
            this.RequireType = requireUserType;
        }
        public RequireUserType? RequireType { get; }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (RequireType.HasValue)
            {
                switch (RequireType.Value)
                {
                    case RequireUserType.FromMention:
                        {
                            if (context.Interaction is SocketMessageComponent componentContext)
                            {
                                if (componentContext.Message.MentionedUsers.Select(a => a.Id).Contains(context.User.Id))
                                    return Task.FromResult(PreconditionResult.FromSuccess());
                            }
                        }
                        break;
                    case RequireUserType.MainGuildOwner:
                        {
                            var config = services.GetRequiredService<Configuration>();
                            var guildId = config.Guilds.MainServer;
                            var ownerId = (context.Client as DiscordSocketClient).GetGuild(guildId).OwnerId;
                            if (ownerId == context.User.Id)
                                return Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        break;
                    case RequireUserType.BotOwner:
                        {
                            var config = services.GetRequiredService<Configuration>();
                            if (config.OwnerId == context.User.Id)
                                return Task.FromResult(PreconditionResult.FromSuccess());
                        }
                        break;
                }
            }

            return Task.FromResult(PreconditionResult.FromError("RequireUser"));
        }
        public enum RequireUserType
        {
            FromMention,
            FromSlashCommand,
            MainGuildOwner,
            BotOwner,
        }
    }
}
