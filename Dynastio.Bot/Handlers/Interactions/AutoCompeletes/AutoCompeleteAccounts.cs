using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Database;

namespace Dynastio.Bot.Interactions
{
    public class AutoCompeleteAccounts : AutocompleteHandler
    {
        public DynastioBotDatabase DynastioData { get; set; }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string match = autocompleteInteraction.Data.Current.Value.ToString();

            var user_ = autocompleteInteraction.Data.Options.Where(a => a.Name == "user").FirstOrDefault();

            User user = user_ is null || string.IsNullOrEmpty((string)user_.Value)
                ? (context as BotSocketInteractionContext).BotUser
                : await DynastioData.GetUserAsync(ulong.Parse((string)user_.Value));

            var accounts = string.IsNullOrEmpty(match)
                ?
                user.Accounts.OrderBy(a => a.AddedAt).ToList()
                :
                user.Accounts.OrderBy(a => a.AddedAt).Where(a => a.Reminder.Contains(match)).ToList();

            var result = new List<AutocompleteResult>();
            foreach (var account in accounts)
            {
                result.Add(new AutocompleteResult()
                {
                    Name = account.Reminder,
                    Value = account.GetHashCode().ToString()
                });
            }

            // max - 25 suggestions at a time (API limit)
            return await Task.FromResult(AutocompletionResult.FromSuccess(result.Take(25)));
        }


    }
}