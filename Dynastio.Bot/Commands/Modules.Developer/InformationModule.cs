using Discord.Commands;
using Dynastio.Bot.Database;
using Dynastio.Bot.Services;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Commands.Developer
{
    [RequireContext(ContextType.Guild)]
    [RequireApplicationTeamAttribute()]
    public class InformationModule : ModuleBase<BotSocketCommandContext>
    {
        public EmoteService EmoteService { get; set; }
        public RankingService RankingService { get; set; }


        [Command("information")]
        public async Task information()
        {
            var msg = await ReplyAsync(" working on it, may takes time .. || This message will disappear itself !||");

            await InformationEmotes();

            await Task.Delay(2000);
            await InformationRoles();

            await msg.DeleteAsync().TryAsync();
        }

        [Command("information_roles")]
        public async Task InformationRoles()
        {

            string header = $"# ● ▬←⋯⋯⋯⋯⋯⊰⊰     Ranking Roles  ⊱⊱⋯⋯⋯⋯▬ ●";
            var allRoles = RoleHelper
                .GetRolesWithPrefix(Context.Guild, Context.BotGuild.RankingSettings.Prefix)
                .ToList();

            var parts = new List<string>();
            string current = header;

            int row = 1;
            foreach (var role in allRoles)
            {
                var requiredXp = XpCalculator.GetCurrentLevelXpRequirement(row);
                string line = $"\n{row.ToRegularCounter()}. {role.Emoji.ToString()}{role.Mention} unlock at level **{row}** --> `require {requiredXp.ToMetric()} Xp` ";

                // If adding this line would exceed 2000 chars, save current and start new
                if (current.Length + line.Length > 2000)
                {
                    parts.Add(current);
                    current = string.Empty;
                }

                current += line;
                row++;
            }

            // Add the last chunk
            if (!string.IsNullOrEmpty(current))
                parts.Add(current);

            // Send each part
            foreach (var part in parts)
            {
                await ReplyAsync(part, allowedMentions: Discord.AllowedMentions.None);
            }



        }

        [Command("information_emotes")]
        public async Task InformationEmotes()
        {
            var types = new string[] { "items", "buildings", "badges", "skins" };
            foreach (var item in types)
            {
                await SendApplicationEmotesAsync(item, 1, 1000);
                await Task.Delay(5000);
            }
        }




        [Command("information_emote")]
        public async Task SendApplicationEmotesAsync(string type = "items", int from = 1, int count = 50)
        {
            var shapeGen = new DynastioShapeGenerator(EmoteService);

            string shape = type switch
            {
                "items" => await shapeGen.CreateEmojiListAsync<ItemType>(from, count, false),
                "buildings" => await shapeGen.CreateEmojiListAsync<EntityType>(from, count, false),
                "badges" => await shapeGen.CreateEmojiListAsync<BadgeType>(from, count, false),
                "skins" => await shapeGen.CreateEmojiListAsync<SkinType>(from, count, false),
                _ => null
            };

            await ReplyAsync($"# ● ▬←⋯⋯⋯⋯⋯⊰⊰     DYNAST.IO " + type.ToUpper() + " ⊱⊱⋯⋯⋯⋯▬ ●");

            foreach (var part in SplitEmoteString(shape, 1900))
            {
                await Task.Delay(2000);
                await ReplyAsync("# " + part);
            }
        }
        public static IEnumerable<string> SplitEmoteString(string input, int maxLength = 3900)
        {
            if (string.IsNullOrWhiteSpace(input))
                yield break;

            var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var sb = new StringBuilder();

            foreach (var token in tokens)
            {
                // +1 for the space we’ll add
                if (sb.Length + token.Length + 1 > maxLength)
                {
                    yield return sb.ToString().TrimEnd();
                    sb.Clear();
                }

                sb.Append(token).Append(' ');
            }

            if (sb.Length > 0)
                yield return sb.ToString().TrimEnd();
        }
    }
}
