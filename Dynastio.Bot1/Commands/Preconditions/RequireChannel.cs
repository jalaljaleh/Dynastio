using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Dynastio.Bot;
using Microsoft.Extensions.DependencyInjection;

namespace Discord.Commands
{
    public class RequireChannel : PreconditionAttribute
    {
        public ulong? ChannelId
        {
            get;
        }
        public RequireChannel(ulong ChannelId)
        {
            this.ChannelId = ChannelId;
        }
        public RequireChannel(LocalChannelId channel)
        {
            this.LocalChannel = channel;
        }
        public enum LocalChannelId
        {
            Honor
        }
        public LocalChannelId? LocalChannel
        {
            get;
        }

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (LocalChannel.HasValue ? (context.Channel.Id == services.GetRequiredService<Configuration>().Channels.HonorChannel) : (context.Channel.Id == ChannelId))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? $"This channel is not supported try in <#{ChannelId.Value}>."));
        }
    }
}
