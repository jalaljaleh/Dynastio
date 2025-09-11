
using Discord.Interactions;
using MongoDB.Bson.IO;
using Newtonsoft.Json;

namespace Dynastio.Bot.Interactions.Modules.Owner
{
    [RequireTeam]
    [RequireUserPermission(Discord.GuildPermission.Administrator)]
    [Group("developer", "developer commands")]
    public class Developer : MenuModulesBase
    {
        [SlashCommand("pincode", "bypass pin code")]
        public async Task ByPassPinCode(string newCode)
        {
            Interactions.Modules.Menu.Buttons.ButtonLoginModule.BypassPinCode = newCode;
            await RespondAsync($"PinCode-bypass created successfuly.", ephemeral: true);
        }

        public DynastioItemsService ItemsService { get; set; }
        public AiChatService Ai { get; set; }

        [SlashCommand("items", "items config")]
        public async Task items(string item)
        {
            await DeferAsync();
            // 1) Lookup
            if (!ItemsService.TryGetItem(item.ToLowerInvariant().Trim(), out var target))
            {
                await FollowupAsync($"❌ Cannot find item `{item}`.", ephemeral: true);
                return;
            }

           
            string systemPrompt = @"
Important:
- You are Dynast.io Bot, the official AI assistant on our Dynast.io Discord server.
- Always base your answers on the Dynast.io game data provided.
- Use clear, non-technical language so all members can understand.
- Apply Discord Markdown for formatting and use @Mentions instead of raw IDs.

Analyze this Dynast.io game item and explain it clearly:
";
           
            // 3) Query
            string aiResponse = await Ai.QueryAsync(null,systemPrompt + Newtonsoft.Json.JsonConvert.SerializeObject(target));

            // 4) Send back
            await FollowupAsync(aiResponse);
        }
    }
}
