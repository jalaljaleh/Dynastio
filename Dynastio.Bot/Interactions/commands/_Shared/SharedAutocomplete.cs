using Discord;
using Discord.Interactions;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynastio.Bot.Interactions;
using Dynastio.Bot.Data;

namespace Dynastio.Bot.Interactions.Modules.Shard
{
    public class SharedAutocompleteHandler
    {
        public class AccountAutocompleteHandler : AutocompleteHandler
        {
            public UserService UserService { get; set; }
            public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
            {
                string match = autocompleteInteraction.Data.Current.Value.ToString();

                var accounts = (context as CustomSocketInteractionContext)
                    .BotUser
                    .Accounts
                    .Where(a => a.Reminder.Contains(match)).ToList();

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


        public class OnlineServersAutocompleteHandler : AutocompleteHandler
        {
            public DynastioClient Dynastio { get; set; }
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
        public class OnlinePrivateServersAutocompleteHandler : AutocompleteHandler
        {
            public DynastioClient Dynastio { get; set; }
            public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
            {
                List<AutocompleteResult> results = new();

                string match = autocompleteInteraction.Data.Current.Value.ToString();

                var servers = Dynastio.OnlineServers.Where(
                    a =>
                    a.IsPrivate == true &&
                    a.Label.ToLower().Contains(match)).Take(25).ToList();

                foreach (var server in servers)
                {
                    results.Add(new AutocompleteResult()
                    {
                        Name = server.Label.RemoveHtmlTags().TryRemove(98),
                        Value = server.GetHashCode().ToString()
                    });
                }
                // max - 25 suggestions at a time (API limit)
                return await Task.FromResult(AutocompletionResult.FromSuccess(results));
            }
        }
        public class OnlinePlayersAutocompleteHandler : AutocompleteHandler
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
}