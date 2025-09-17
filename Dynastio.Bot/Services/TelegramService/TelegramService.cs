using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Dynastio.Bot
{

    public sealed class TelegramBotService
    {
        private readonly TelegramBotClient _client;
        public TelegramBotService(string token)
        {
            _client = new TelegramBotClient(new TelegramBotClientOptions(token, "https://telegram-api-proxy-anonymous.pages.dev/api/bot/"));
            
        }
        public async Task<bool> SendMessageAsync(string username, string message)
        {
            var res = await _client.SendMessage(new ChatId(username), message, Telegram.Bot.Types.Enums.ParseMode.Html)
                .TryAsync();

            return res.isSuccessful;
        }
        public async Task<bool> SendPhotoMessageAsync(string username, string imageUrl, string caption)
        {
            var res = await _client.SendPhoto(new ChatId(username),InputFile.FromUri(imageUrl),caption, Telegram.Bot.Types.Enums.ParseMode.Html)
                .TryAsync();

            return res.isSuccessful;
        }
    }

}
