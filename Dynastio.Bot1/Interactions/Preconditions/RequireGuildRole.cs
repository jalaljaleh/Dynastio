/*!
 * Discord Precondition Require_Guild_Role  v1.1 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Dynastio.Discord/blob/master/Dynastio.Bot/Interactions/Preconditions/RequireGuildRole.cs)
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

    public class RequireGuildRole : PreconditionAttribute
    {
        public ulong? GuildRole
        {
            get;
        }
        public ulong? GuildId
        {
            get;
        }
        public RequireGuildRole(ulong RoleId, ulong GuildId)
        {
            this.GuildRole = RoleId;
            this.GuildId = GuildId;
        }
        public RequireGuildRole(ulong RoleId)
        {
            this.GuildRole = RoleId;
        }
     
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
         
            var guild = context.Guild as SocketGuild;
            if (this.GuildId.HasValue) guild = (context.Client as DiscordSocketClient).Guilds.Where(a => a.Id == this.GuildId).FirstOrDefault();

            if (guild == null) return Task.FromResult(PreconditionResult.FromError("Guild Not Found."));

            var user = guild.GetUser(context.User.Id);
            if (user == null) return Task.FromResult(PreconditionResult.FromError("User Not Found."));

            if (user.Roles.Where(r => r.Id == this.GuildRole).Any())
                return Task.FromResult(PreconditionResult.FromSuccess());

            return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage ?? "You need guild role."));
        }
    }
}
