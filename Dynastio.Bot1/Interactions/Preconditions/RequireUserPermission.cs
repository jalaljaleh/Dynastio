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
using Dynastio.Data;

namespace Discord.Interactions
{
    public class RequireBotUserPermissionAttribute : PreconditionAttribute
    {
        public RequireBotUserPermissionAttribute(RequirePermissionType require)
        {
            this.RequireType = require;
        }
        public RequirePermissionType? RequireType { get; }
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            ICustomInteractionContext _context = context as ICustomInteractionContext;
            bool permission = RequireType.Value switch
            {
                RequirePermissionType.UseBot => _context.BotUser.IsBanned,
                RequirePermissionType.AddPrivateServerCommand => _context.BotUser.IsBannedToAddNewPrivateServerCommand,
                RequirePermissionType.AddAccount => _context.BotUser.IsBannedToAddNewAccount,
                _ => false
            };
            if (permission)
                return Task.FromResult(PreconditionResult.FromError("You do not have the required access, ask the administrator to grant you access."));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
        public enum RequirePermissionType
        {
            AddAccount,
            UseBot,
            AddPrivateServerCommand
        }
    }
}
