using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Extenstions;
using Dynastio.Bot.Extenstions;

namespace Dynastio.Bot.Interactions.AutoCompeletes
{


    public class AutoCompeleteOnlineServers : AutocompleteHandler
    {
        public DynastioApi Dynastio { get; set; }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> results = new();

            string match = autocompleteInteraction.Data.Current.Value.ToString();


            var servers = Dynastio.OnlineServers.Where(
                a =>
                a.IsPrivate == false &&
                a.Label.ToLower().Contains(match)).Take(25).ToList();

            foreach (var server in servers)
            {
                results.Add(new AutocompleteResult()
                {
                    Name = server.Label.TryRemove(98),
                    Value = server.Label.TryRemove(30, false)
                });
            }
            // max - 25 suggestions at a time (API limit)
            return await Task.FromResult(AutocompletionResult.FromSuccess(results));
        }
    }

}