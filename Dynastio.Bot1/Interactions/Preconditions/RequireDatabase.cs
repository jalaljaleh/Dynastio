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
using Dynastio.Data;
namespace Discord.Interactions
{
    public class RequireDatabase : PreconditionAttribute
    {
        public RequireDatabase()
        {
        }
       
        public new string ErrorMessage = $"This command require database.";
        public override Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services)
        {
            var db = services.GetRequiredService<Dynastio.Data.IDatabaseContext>();
            if (db is NoDatabaseDbContext)
                return Task.FromResult(PreconditionResult.FromError(this.ErrorMessage));
            else
                return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
