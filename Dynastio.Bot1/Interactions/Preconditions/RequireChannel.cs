/*!
 * Discord Precondition Require Channel v1 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under Apache (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Interactions/Preconditions/RequireChannel.cs)
 */

namespace Discord.Interactions
{
﻿using Discord.Interactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;
using Microsoft.Extensions.DependencyInjection;

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
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            if (LocalChannel.HasValue ? (context.Channel.Id == services.GetRequiredService<Dynastio.Bot.Configuration>().Channels.HonorChannel) : (context.Channel.Id == ChannelId))
                return Task.FromResult(PreconditionResult.FromSuccess());
            else
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? $"This channel is not supported try in <#{ChannelId.Value}>."));
        }
    }
}
