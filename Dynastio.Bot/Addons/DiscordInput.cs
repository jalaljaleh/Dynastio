/*!
 * Discord Input v1.0 (https://jalaljaleh.github.io/)
 * Copyright 2021-2022 Jalal Jaleh
 * Licensed under MIT (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/LICENSE.txt)
 * Original (https://github.com/jalaljaleh/Template.Discord.Bot/blob/master/Template/Utilities/DiscordInput.cs)
 */

namespace Dynastio.Bot.Addons
{
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using System;
    using System.Threading.Tasks;
    public static class DiscordInput
    {
        public static async Task<SocketMessage> WaitForContextMessageAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, Timeout);
        public static async Task<SocketMessage> WaitForUserMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong UserId, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, Timeout);
        public static async Task<SocketMessage> WaitForChannelMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong ChannelId, bool OnlyCurrentGuild = true)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, Context.Channel.Id, 0, Timeout);
        public static async Task<SocketMessage> WaitForGuildMessageAsync(this ICommandContext Context, TimeSpan Timeout, ulong GuildId)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, Context.Guild.Id, 0, 0, Timeout);
        public static async Task<SocketMessage> WaitForMessageAsync(this ICommandContext Context, TimeSpan Timeout)
       => await WaitForMessageAsync(Context.Client as DiscordSocketClient, 0, 0, 0, Timeout);

        public static async Task<SocketMessage> WaitForContextMessageAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, Timeout);
        public static async Task<SocketMessage> WaitForUserMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong UserId, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, Timeout);
        public static async Task<SocketMessage> WaitForChannelMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong ChannelId, bool OnlyCurrentGuild = true)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, Context.Channel.Id, 0, Timeout);
        public static async Task<SocketMessage> WaitForGuildMessageAsync(this IInteractionContext Context, TimeSpan Timeout, ulong GuildId)
        => await WaitForMessageAsync(Context.Client as DiscordSocketClient, Context.Guild.Id, 0, 0, Timeout);
        public static async Task<SocketMessage> WaitForMessageAsync(this IInteractionContext Context, TimeSpan Timeout)
       => await WaitForMessageAsync(Context.Client as DiscordSocketClient, 0, 0, 0, Timeout);

        public static async Task<SocketMessage> WaitForMessageAsync
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
                    GuildId != 0 && message is ITextChannel && (message as ITextChannel).GuildId != GuildId ||
                    ChannelId != 0 && ChannelId != message.Channel.Id ||
                    AuthorId != 0 && AuthorId != message.Author.Id)
                        return Task.CompletedTask;

                    inputTask.TrySetResult(message);
                    return Task.CompletedTask;
                });
                return Task.CompletedTask;
            }
        }


        public static async Task<SocketMessageComponent> WaitForContextMessageComponentAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForMessageComponentFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForMessageComponentFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForMessageComponentFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserMessageComponentFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> WaitForContextMessageComponentAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForMessageComponentFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForMessageComponentFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForMessageComponentFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, Optional<ComponentType>.Unspecified);
        public static async Task<SocketMessageComponent> WaitForUserMessageComponentFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserMessageComponentFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> WaitForContextButtonAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
    => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForButtonFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForButtonFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForButtonFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserButtonAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForUserButtonFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForUserButtonFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserButtonFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> WaitForContextButtonAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
          => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForButtonFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForButtonFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForButtonFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserButtonAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForUserButtonFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, ComponentType.Button);
        public static async Task<SocketMessageComponent> WaitForUserButtonFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserButtonFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);


        public static async Task<SocketMessageComponent> WaitForContextSelectMenuAsync(this IInteractionContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
          => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForSelectMenuFromMessageIdAsync(this IInteractionContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForSelectMenuFromMessageAsync(this IInteractionContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForSelectMenuFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuAsync(this IInteractionContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuFromMessageIdAsync(this IInteractionContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuFromMessageAsync(this IInteractionContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserSelectMenuFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> WaitForContextSelectMenuAsync(this ICommandContext Context, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
         => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForSelectMenuFromMessageIdAsync(this ICommandContext Context, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, OnlyCurrentUser ? Context.User.Id : 0, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForSelectMenuFromMessageAsync(this ICommandContext Context, IMessage Message, TimeSpan Timeout, bool OnlyCurrentUser = true, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
       => await WaitForSelectMenuFromMessageIdAsync(Context, Message.Id, Timeout, OnlyCurrentUser, OnlyCurrentChannel, OnlyCurrentGuild);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuAsync(this ICommandContext Context, ulong UserId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
      => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, 0, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuFromMessageIdAsync(this ICommandContext Context, ulong UserId, ulong MessageId, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForMessageComponentAsync(Context.Client as DiscordSocketClient, OnlyCurrentGuild ? Context.Guild.Id : 0, OnlyCurrentChannel ? Context.Channel.Id : 0, UserId, MessageId, Timeout, ComponentType.SelectMenu);
        public static async Task<SocketMessageComponent> WaitForUserSelectMenuFromMessageAsync(this ICommandContext Context, ulong UserId, IMessage Message, TimeSpan Timeout, bool OnlyCurrentChannel = true, bool OnlyCurrentGuild = true)
     => await WaitForUserSelectMenuFromMessageIdAsync(Context, UserId, Message.Id, Timeout, OnlyCurrentChannel, OnlyCurrentGuild);

        public static async Task<SocketMessageComponent> WaitForMessageComponentAsync(DiscordSocketClient client, ulong GuildId, ulong ChannelId, ulong UserId, ulong MessageId, TimeSpan Timeout, Optional<ComponentType> ComponentType)
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
                        if (!IsFromDiscordInput(arg))
                            return Task.CompletedTask;

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

        public static async Task<SocketModal> WaitForContextModalAsync(this ICommandContext Context, TimeSpan Timeout)
        => await WaitForModalAsync(Context.Client as DiscordSocketClient, Context.User.Id, Timeout);
        public static async Task<SocketModal> WaitForContextModalAsync(this IInteractionContext Context, TimeSpan Timeout)
        => await WaitForModalAsync(Context.Client as DiscordSocketClient, Context.User.Id, Timeout);

        public static async Task<SocketModal> WaitForModalAsync(DiscordSocketClient client, ulong UserId, TimeSpan Timeout)
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

                    if (arg.Type != InteractionType.ModalSubmit || arg.User.Id != UserId) return Task.CompletedTask;

                    if (!IsFromDiscordInput(arg))
                        return Task.CompletedTask;

                    inputTask.TrySetResult(arg as SocketModal);
                    return Task.CompletedTask;
                });
                return Task.CompletedTask;
            }
        }
        public const string CustomId = "_discord-input: ";
        public static string GenerateCustomId(string customId) => CustomId + customId;
        public static string GetCustomId(string customId) => CustomId + customId;
        public static bool IsFromDiscordInput(SocketInteraction interaction)
        {
            return interaction.Type switch
            {
                InteractionType.ModalSubmit => (interaction as SocketModal).Data.CustomId.StartsWith(CustomId),
                InteractionType.MessageComponent => (interaction as SocketMessageComponent).Data.CustomId.StartsWith(CustomId),
                _ => false
            };
        }
    }
}