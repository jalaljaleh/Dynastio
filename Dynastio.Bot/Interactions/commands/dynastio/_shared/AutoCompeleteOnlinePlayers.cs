using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Data;

namespace Dynastio.Bot.Interactions.commands.dynastio._shared
{

    public class AutoCompeleteOnlinePlayers : AutocompleteHandler
    {
        public DynastioClient Dynastio { get; set; }
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> results = new();

            string match = autocompleteInteraction.Data.Current.Value.ToString().ToLower();
            var server = autocompleteInteraction.Data.Options.FirstOrDefault(a => a.Name == "server");

            var players = Dynastio.OnlineServers.Where(
                a => server != null ? a.Label.ToLower().Contains((string)server.Value) : true).ToList()
                .SelectMany(a => a.Players)
                .Where(a => a.Nickname.ToLower().Contains(match)).Take(25).ToList();

            foreach (var player in players)
            {
                results.Add(new AutocompleteResult()
                {
                    Name = player.Nickname.TryRemove(16),
                    Value = player.UniqeId
                });
            }
            // max - 25 suggestions at a time (API limit)
            return await Task.FromResult(AutocompletionResult.FromSuccess(results));
        }
    }

}