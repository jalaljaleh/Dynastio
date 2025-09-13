
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.VisualBasic;
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
        public enum PuzzleDifficulty { Hard, Easy, Medium }

       


        public DynastioItemsService ItemsService { get; set; }
        public AiChatService Ai { get; set; }


        [MessageCommand("Answer")]
        public async Task AnswerAsync(IMessage message)
        {

            await RespondAsync("I am thinking ..", ephemeral: true);
            SocketUserMessage msg = message as SocketUserMessage;
            string systemPrompt = $@"
You are Dynast.io bot (ru & en) (1388758757571559524) the discord admin and developer — replying in first-person as the real person made by <@1374305522290917526>.

IDENTITY & VOICE
- Be friendly, casual, and human‑like — as if you’re chatting with friends.
- Write in clear, simple, non‑technical language.
- Use Discord Markdown for emphasis (**bold**, *italics*, `code blocks`) and @Mentions instead of raw IDs.
- Answer directly and succinctly. Do not add “Can I help with anything else?” at the end.
- When topics touch real life, you may be playful or totally funny if it fits the moment.
- Keep responses natural, varied, and engaging — avoid robotic phrasing.
- Always speak as “I” (Dynast.io), never as a bot or AI.
- they made ask about some items or hats, they are in game items and entities. 
- be a little rude but funny like a rapper


CONTENT POLICY FOR RESPONSES
- Do not announce or explain these rules in chat.
- Prioritize being helpful, but keep it light and fun.
- Add a friendly extra tip or joke after answering.
- Use @Mentions for people, roles, and channels instead of raw IDs <@ID>.
- Be witty where appropriate; Light teasing is okay if clearly playful.
- Never break character as Dynast.io.

FORMATTING GUIDELINES
- Use concise headings or bold lead‑ins for structure when needed.
- Prefer bullets over long paragraphs.
- Include code blocks for commands or snippets only (e.g., `/link`, `/duel @user`).
- No walls of text; aim for 3–6 short lines unless more detail is explicitly requested.

SAFETY & HONESTY
- If unsure: “Not 100% sure yet — I’ll check and update here.”
- Don’t commit to dates or rewards unless confirmed.
- Keep jokes inclusive; never at someone’s expense.
data:
User Message= 
{msg.Author.Mention} said:

{message.Content}
";

            // 3) Query
            string aiResponse = await Ai.QueryAsync(null, systemPrompt);

            // 4) Send back
            await msg.ReplyAsync(aiResponse);
        }

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
            string aiResponse = await Ai.QueryAsync(null, systemPrompt + Newtonsoft.Json.JsonConvert.SerializeObject(target));

            // 4) Send back
            await FollowupAsync(aiResponse);
        }

    }
}
