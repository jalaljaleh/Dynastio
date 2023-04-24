/*!
 * Discord Input v1.0 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/Template/Utilities/DiscordInput.cs)
 */

namespace Dynastio.Bot {
    
 using Discord;
 using Discord.Commands;
 using Discord.WebSocket;
 using Dynastio.Bot;
 using System;
 using System.Collections.Generic;
 using System.Linq;
 using System.Text;
 using System.Threading.Tasks;
 using Dynastio.Data;  
 
    public static class DiscordInput
    {


        public static async Task<SocketMessage> ReadContextMessageAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), Timeout);
        public static async Task<SocketMessage> ReadUserMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong UserId, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, Timeout);
        public static async Task<SocketMessage> ReadChannelMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong ChannelId, bool OnlyCurrentGuild = true)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), Context.Channel.Id, 0, Timeout);
        public static async Task<SocketMessage> ReadGuildMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong GuildId)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, Context.Guild.Id, 0, 0, Timeout);
        public static async Task<SocketMessage> ReadMessageAsync(this ICommandContext Context, TimeSpan Timeout)
       => await ReadMessageAsync(Context.Client as DiscordSocketClient, 0, 0, 0, Timeout);

        public static async Task<SocketMessage> ReadContextMessageAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), Timeout);
        public static async Task<SocketMessage> ReadUserMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong UserId, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, Timeout);
        public static async Task<SocketMessage> ReadChannelMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong ChannelId, bool OnlyCurrentGuild = true)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), Context.Channel.Id, 0, Timeout);
        public static async Task<SocketMessage> ReadGuildMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong GuildId)
        => await ReadMessageAsync(Context.Client as DiscordSocketClient, Context.Guild.Id, 0, 0, Timeout);
        public static async Task<SocketMessage> ReadMessageAsync(this IInteractionContext Context, TimeSpan Timeout)
       => await ReadMessageAsync(Context.Client as DiscordSocketClient, 0, 0, 0, Timeout);

        public static async Task<SocketMessage> ReadMessageAsync
            (DiscordSocketClient client, ulong GuildId, ulong ChannelId, ulong AuthorId, TimeSpan Timeout)
        {
            var inputTask = new TaskCompletionSource<SocketMessage>();
            try
            {
                client.MessageReceived += Dsc_MessageReceived;

                if (await Task.WhenAny(inputTask.Task, Task.Delay(Timeout)).ConfigureAwait(false) != inputTask.Task) return null;

                return await inputTask.Task.ConfigureAwait(false);
            }
            finally
            {
                client.MessageReceived -= Dsc_MessageReceived;
            }

            Task Dsc_MessageReceived(SocketMessage message)
            {
                _ = Task.Run(() =>
                {
                    if (
                    (GuildId != 0 && message is ITextChannel && (message as ITextChannel).GuildId != GuildId) ||
                    (ChannelId != 0 && ChannelId != message.Channel.Id) ||
                    (AuthorId != 0 && AuthorId != message.Author.Id))
                        return Task.CompletedTask;

                    inputTask.TrySetResult(message);
                    return Task.CompletedTask;
                });
                return Task.CompletedTask;
            }
        }


        public static async Task<SocketMessageComponent> ReadContextMessageComponentAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadMessageComponentFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadMessageComponentFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadMessageComponentFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserMessageComponentFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> ReadContextMessageComponentAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadMessageComponentFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadMessageComponentFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadMessageComponentFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> ReadUserMessageComponentFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserMessageComponentFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> ReadContextButtonAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
    => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadButtonFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadButtonFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadButtonFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserButtonAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadUserButtonFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadUserButtonFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserButtonFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> ReadContextButtonAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
          => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadButtonFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadButtonFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadButtonFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserButtonAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadUserButtonFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> ReadUserButtonFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserButtonFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> ReadContextSelectMenuAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
          => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadSelectMenuFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadSelectMenuFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadSelectMenuFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserSelectMenuFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> ReadContextSelectMenuAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
         => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadSelectMenuFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), (OnlyCurrentUser ? Context.User.Id : 0), MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadSelectMenuFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await ReadSelectMenuFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadMessageComponentAsync(Context.Client as DiscordSocketClient, (OnlyCurrentGuild ? Context.Guild.Id : 0), (OnlyCurrentChannel ? Context.Channel.Id : 0), UserId, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> ReadUserSelectMenuFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await ReadUserSelectMenuFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> ReadMessageComponentAsync(DiscordSocketClient client, ulong GuildId, ulong ChannelId, ulong UserId, ulong MessageId, TimeSpan Timeout, Optional<ComponentType> ComponentType)
        {
            var inputTask = new TaskCompletionSource<SocketMessageComponent>();
            try
            {
                client.InteractionCreated += Client_InteractionCreated; ;

                if (await Task.WhenAny(inputTask.Task, Task.Delay(Timeout)).ConfigureAwait(false) != inputTask.Task) return null;

                return await inputTask.Task.ConfigureAwait(false);
            }
            finally
            {
                client.InteractionCreated -= Client_InteractionCreated;
            }

            Task Client_InteractionCreated(SocketInteraction arg)
            {
                _ = Task.Run(() =>
                {
                    //if (InteractionUtilities.IsStaticInteractionCommand(arg)) return Task.CompletedTask; ;
                    if (arg is SocketMessageComponent Smc)
                    {
                        if (GuildId != 0 && Smc.GuildId != GuildId || ChannelId != 0 && Smc.ChannelId != ChannelId || UserId != 0 && Smc.User.Id != UserId || MessageId != 0 && Smc.Message.Id != MessageId || ComponentType.IsSpecified && Smc.Data.Type != ComponentType.Value)
                            return Task.CompletedTask;

                        inputTask.TrySetResult(arg as SocketMessageComponent);
                        return Task.CompletedTask;
                    }
                    else return Task.CompletedTask;
                });
                return Task.CompletedTask;
            }
        }

        public static async Task<SocketModal> ReadContextModalAsync(this ICommandContext Context, TimeSpan Timeout)
        => await ReadModalAsync(Context.Client as DiscordSocketClient, Context.User.Id, Timeout);
        public static async Task<SocketModal> ReadContextModalAsync(this IInteractionContext Context, TimeSpan Timeout)
        => await ReadModalAsync(Context.Client as DiscordSocketClient, Context.User.Id, Timeout);

        public static async Task<SocketModal> ReadModalAsync(DiscordSocketClient client, ulong UserId, TimeSpan Timeout)
        {
            var inputTask = new TaskCompletionSource<SocketModal>();
            try
            {
                client.ModalSubmitted += Client_ModalSubmitted; ;

                if (await Task.WhenAny(inputTask.Task, Task.Delay(Timeout)).ConfigureAwait(false) != inputTask.Task) return null;

                return await inputTask.Task.ConfigureAwait(false);
            }
            finally
            {
                client.ModalSubmitted -= Client_ModalSubmitted;
            }

            Task Client_ModalSubmitted(SocketInteraction arg)
            {
                _ = Task.Run(() =>
                {
                    if ((arg.Type != InteractionType.ModalSubmit) || (arg.User.Id != UserId)) return Task.CompletedTask;

                    inputTask.TrySetResult(arg as SocketModal);
                    return Task.CompletedTask;
                });
                return Task.CompletedTask;
            }
        }

    }
}
