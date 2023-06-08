using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Data;

namespace Dynastio.Bot.Interactions.AutoCompeletes
{
    public class AutoCompeleteAccounts : AutocompleteHandler
    {
        public UserService UserService { get; set; }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            string match = autocompleteInteraction.Data.Current.Value.ToString();


            var accounts = string.IsNullOrEmpty(match)
                ?
                (context as CustomSocketInteractionContext).BotUser.Accounts.OrderBy(a=>a.AddedAt).ToList()
                :
                (context as CustomSocketInteractionContext).BotUser.Accounts.OrderBy(a => a.AddedAt).Where(a => a.Reminder.Contains(match)).ToList();

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