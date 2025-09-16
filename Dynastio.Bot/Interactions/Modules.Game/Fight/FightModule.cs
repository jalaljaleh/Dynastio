using Amazon.Util.Internal;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Dynastio.Bot.Interactions.Precondinations;
using Dynastio.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot.Interactions.Modules.Game.Fight
{
    [RequireContext(ContextType.Guild)]
    public class FightModule : MenuModulesBase
    {
        // Injected services
        public DynastioItemsService ItemsService { get; set; }

        // Component IDs
        private const string ButtonJoinId = "interactions.buttons.game.pvp.join";
        private const string ButtonStartId = "interactions.buttons.game.pvp.start";

        // Random generator for tie-breakers or demos
        private static readonly Random Rng = new Random();

        // Arena intro text
        private const string ArenaIntro =
            "# ⚔️ PvP Arena ⚔️\n" +
            "Brave warriors, assemble!\n\n" +
            "• Click **Join 🏃** to enter the fray.\n" +
            "• When all fighters are ready, click **Start ⚔️** to begin.\n";


        [SlashCommand("pvp", "⚔️ Open the PvP arena and let players join!")]
        public async Task ExecuteAsync()
        {
            await DeferAsync(ephemeral: false);

            var components = new ComponentBuilder()
                .WithButton("Join 🏃", ButtonJoinId, ButtonStyle.Success)
                .WithButton("Start ⚔️", ButtonStartId, ButtonStyle.Danger)
                .Build();

            await FollowupAsync(ArenaIntro, components: components);
        }

        [ComponentInteraction(ButtonJoinId)]
        [RequireLinkedAccount]
        public async Task JoinAsync()
        {
            var comp = (SocketMessageComponent)Context.Interaction;
            await comp.DeferAsync(ephemeral: true);

            // If already joined, inform and exit
            if (comp.Message.MentionedUsers.Any(u => u.Id == Context.User.Id))
            {
                await FollowupAsync("❗ You’ve already joined the arena!", ephemeral: true);
                return;
            }

            // Build a unique list of fighters
            var fighters = comp.Message.MentionedUsers
                              .Append(Context.User)
                              .DistinctBy(u => u.Id)
                              .ToList();

            // Update arena text once
            var updated = new StringBuilder(ArenaIntro)

                .AppendLine("\n---\n")
                .AppendLine($"**Fighters ({fighters.Count}):**\n>>> {string.Join("\n", fighters.Select(u => $"{fighters.IndexOf(u).ToRegularCounter()}. {u.Mention}"))}")
                .ToString();

            // Preserve original buttons
            var components = comp.Message.Components.ToBuilder().Build();

            await comp.Message.ModifyAsync(msg =>
            {
                msg.Content = updated;
                msg.Components = components;
            });
        }

        [ComponentInteraction(ButtonStartId)]
        [RequireMessageComponentOwner]
        public async Task StartAsync()
        {
            var comp = (SocketMessageComponent)Context.Interaction;
            await comp.DeferAsync(ephemeral: false);

            // Gather distinct players
            var players = comp.Message.MentionedUsers
                               .Append(Context.User)
                               .DistinctBy(u => u.Id)
                               .OfType<SocketUser>()
                               .ToList();

            if (players.Count < 2)
            {
                await FollowupAsync("⚠️ You need at least **2 fighters** to start!", ephemeral: true);
                return;
            }

            // Countdown: 3 → 1
            await Task.Delay(1000);
            for (int i = 3; i > 0; i--)
            {
                await ReplyOrModifyAsync($"#⏳ Tournament begins in **{i}**...");
                await Task.Delay(3000);
            }

            // Initialize log builders
            var logBuilder = new StringBuilder()
                .AppendLine("# 🥳 Tournament Begins! 🥳")
                .AppendLine($"**Participants ({players.Count}):** {string.Join(' ', players.Select(p => p.Mention))}\n");
            var fightHistory = new StringBuilder("## ⚔️ Fight History:\n");
            var eliminationQ = new Queue<SocketUser>(players);
            int roundNumber = 1;

            await ReplyOrModifyAsync(logBuilder.ToString());

            // Single-elimination loop
            while (eliminationQ.Count > 1)
            {
                // Dequeue two fighters
                var p1 = eliminationQ.Dequeue();
                var p2 = eliminationQ.Dequeue();

                // Profiles & chests fetched once per fighter
                var profile1 = await Context.UsersService.GetOrCreateUserAsync(p1.Id);
                var profile2 = await Context.UsersService.GetOrCreateUserAsync(p2.Id);

                var chest1 = await profile1.GetDefaultAccount()?
                                    .GetCachedPersonalChestAsync(Context.Dynastio)
                              ?? new PersonalChest(new() { new PersonalChestItem { ItemType = ItemType.Nothing } });

                var chest2 = await profile2.GetDefaultAccount()?
                                    .GetCachedPersonalChestAsync(Context.Dynastio)
                              ?? new PersonalChest(new() { new PersonalChestItem { ItemType = ItemType.Nothing } });

                // Announce round and inventory
                await ReplyOrModifyAsync(
                    $"# ⚔️ PvP Arena ⚔️\n" +
                    $"# **Round {roundNumber}:**\n{p1.Mention} vs {p2.Mention}");
                await Task.Delay(2000);

                var emotes1 = string.Join(" ", chest1.Items.Select(i => EmoteService.GetEmote(i.ItemType)));
                var emotes2 = string.Join(" ", chest2.Items.Select(i => EmoteService.GetEmote(i.ItemType)));


                string c = $"# 🎒 Inventory:";
                string p11 = $"\n## {p1.Mention}:\n# {string.Join(" ", chest1.Items.DistinctBy(a => a.ItemType).Select(a => EmoteService.GetEmote(a.ItemType)))}  \n";
                string p21 = $"\n## {p2.Mention}:\n# {string.Join(" ", chest2.Items.DistinctBy(a => a.ItemType).Select(a => EmoteService.GetEmote(a.ItemType)))}  \n";


                await ReplyOrModifyAsync(text: "", embeds:new Embed[] {p11.ToEmbed(null,p1.TryGetAvatarUrl()),p21.ToEmbed(null, p2.TryGetAvatarUrl()) });
                await Task.Delay(8000);

                // Compute scores safely
                double score1 = ComputeScore(chest1.Items, ItemsService);
                double score2 = ComputeScore(chest2.Items, ItemsService);


                // Determine outcome
                var winner = score1 >= score2 ? p1 : p2;
                var loser = winner == p1 ? p2 : p1;

                //if (loser.Id == 1374305522290917526)
                //    winner = loser;

                await ReplyOrModifyAsync(
                   $"# ⚔️ PvP Arena ⚔️\n" +
                    $"## ⚔️ {p1.Mention} and {p2.Mention} clash !", embed: null);
                fightHistory.AppendLine($"• {winner.Mention} defeated {loser.Mention}");
                await Task.Delay(6000);

                await ReplyOrModifyAsync(
                     $"# ⚔️ PvP Arena ⚔️\n" +
                    $"## 🎉 Winner: 🏅 {winner.Mention}");
                eliminationQ.Enqueue(winner);
                roundNumber++;

                await Task.Delay(4000);
            }

            // Announce champion
            await Task.Delay(8000);
            var champ = eliminationQ.Dequeue();
            logBuilder.Clear()
                      .AppendLine("# 🎉 Tournament Concluded 🎉")
                      .AppendLine($"**🏆 Champion: {champ.Mention}!**\n")
                      .Append(fightHistory);

            await ReplyOrModifyAsync(logBuilder.ToString());
        }

        // Safely computes a total “power score” from chest items
        private static double ComputeScore(IEnumerable<PersonalChestItem> items, DynastioItemsService svc)
        {
            // Return zero if inputs are missing
            if (items == null || svc == null)
                return 0L;

            double total = 0L;

            foreach (var chestItem in items)
            {
                // Fetch the item definition and its action, skip if missing
                if (svc.GetItem(chestItem.ItemType) is not { ItemAction: var action })
                    continue;

                // Cast numeric fields once, then apply weights

                double damage = action.Damage ?? 0;
                double health = action.Health ?? 0;

                double fire = action.MakeFire is true ? 10 : 0L;
                if (fire > 0)
                    damage = damage * 0.3;

                double power = action.Power ?? 0;
                double weight = action.Weight ?? 0;
                double stamina = action.Stamina ?? 0;

                // Combine into the running total
                total += damage * 4L
                      + health
                      + fire
                      + power
                      - weight
                      - stamina;
            }

            return total;
        }

    }
}
