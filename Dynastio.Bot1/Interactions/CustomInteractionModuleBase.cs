using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.Interactions.Builders;
using Discord.Rest;
using Discord.WebSocket;

namespace Dynastio.Bot.Interactions
{
    //
    // Summary:
    //     Provides a base class for a command module to inherit from.
    //
    // Type parameters:
    //   T:
    //     Type of interaction context to be injected into the module.
    public abstract class CustomInteractionModuleBase<T> : IInteractionModuleBase where T : class, ICustomInteractionContext
    {
        public string this[string key] { get => Context.Locale[key]; }
        public string this[string key, string param1] { get => Context.Locale[key, param1]; }
        public string this[string key, int param1] { get => Context.Locale[key, param1]; }

        //
        // Summary:
        //     Gets the underlying context of the command.
        public T Context { get; private set; }

        public async Task<bool> ModifyToWaiting(int Timeout, IUserMessage message = null)
        {
            await (message ?? (Context.Interaction as SocketMessageComponent).Message).ModifyAsync(x =>
            {
                x.Content = (message ?? (Context.Interaction as SocketMessageComponent).Message).MentionedUserIds.FirstOrDefault().ToUserMention();
                x.Embed = new EmbedBuilder()
                {
                    Title = this["waiting_for_user"],
                    Description = this["waiting_for_user"] + $" \n\n" +
                                  this["closes:*", Timeout.ToDiscordUnixTimeFromat()],
                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/916998929609023509/917508918991802448/2349488.png",
                    Color = Color.LightOrange
                }.Build();
                x.Embeds = new Embed[] { };
                x.Attachments = new Optional<IEnumerable<FileAttachment>>();
                x.Components = new ComponentBuilder().Build();
            });
            return true;
        }
        public async Task<bool> ModifyToClosed(IUserMessage message = null)
        {
            await (message ?? message ?? (Context.Interaction as SocketMessageComponent).Message).ModifyAsync(x =>
            {
                x.Content = (message ?? (Context.Interaction as SocketMessageComponent).Message).MentionedUserIds.FirstOrDefault().ToUserMention();
                x.Embed = new EmbedBuilder()
                {
                    Title = this["menu_closed.title"],
                    Description = this["menu_closed.description:*", "/menu"] + "\n" +
                                  $"\n[{this["support_server"]}](https://discord.gg/GVUXMNv7vV) | [{this["youtube_channel"]}](https://www.youtube.com/channel/UCW0PmC1B8jjhpKLHciFp0xA/?sub_confirmation=1)",
                    ThumbnailUrl = "https://cdn.discordapp.com/attachments/916998929609023509/967515051718221824/dynastio.png",
                    Color = Color.Blue
                }.Build();
                x.Embeds = new Embed[] { };
                x.Attachments = new Optional<IEnumerable<FileAttachment>>();
                x.Components = new ComponentBuilder().Build();
            });
            return true;
        }
        public async Task<bool> ModifyToLoaded(string Title = null, string Description = null, string ThumbnailUrl = null, IUserMessage message = null)
        {
            await (message ?? (Context.Interaction as SocketMessageComponent).Message).ModifyAsync(x =>
            {
                x.Content = (message ?? (Context.Interaction as SocketMessageComponent).Message).MentionedUserIds.FirstOrDefault().ToUserMention();
                x.Embed = new EmbedBuilder()
                {
                    Title = Title ?? this["successful_operation.title"],
                    Description = Description ?? this["successful_operation.description"] + $"\n",
                    ThumbnailUrl = ThumbnailUrl ?? "https://cdn.discordapp.com/attachments/916998929609023509/967515051718221824/dynastio.png",
                    Color = Color.Green
                }.Build();
                x.Embeds = new Embed[] { };
                x.Attachments = new Optional<IEnumerable<FileAttachment>>();
                x.Components = new ComponentBuilder().Build();
            });
            return true;
        }
        public async Task<bool> ToNoComponent(IUserMessage message = null)
        {
            await (message ?? (Context.Interaction as SocketMessageComponent).Message).ModifyAsync(x =>
            {
                x.Components = new ComponentBuilder().Build();
            });
            return true;
        }
        public async Task<bool> ModifyToLoading(string Title = null, string Description = null, string ThumbnailUrl = null, IUserMessage message = null)
        {
            await (message ?? (Context.Interaction as SocketMessageComponent).Message).ModifyAsync(x =>
            {
                x.Content = (message ?? (Context.Interaction as SocketMessageComponent).Message).MentionedUserIds.FirstOrDefault().ToUserMention();
                x.Embed = new EmbedBuilder()
                {
                    Title = Title ?? this["waiting_for_bot.title"],
                    Description = Description ?? this["waiting_for_bot.description"] + "\n" +
                                       $"\n\n" + this["waiting_since:*", DateTime.UtcNow.ToDiscordUnixTimestampFormat()].ToBold(),
                    ThumbnailUrl = ThumbnailUrl ?? "https://cdn.discordapp.com/attachments/916998929609023509/987005106203553812/Ellipsis-4.5s-183px_1.gif",
                    Color = Color.LightOrange
                }.Build();
                x.Embeds = new Embed[] { };
                x.Attachments = new Optional<IEnumerable<FileAttachment>>();
                x.Components = new ComponentBuilder().Build();
            });
            return true;
        }
        public virtual void AfterExecute(ICommandInfo command)
        {
        }

        public virtual void BeforeExecute(ICommandInfo command)
        {
        }

        public virtual Task BeforeExecuteAsync(ICommandInfo command)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterExecuteAsync(ICommandInfo command)
        {
            return Task.CompletedTask;
        }

        public virtual void OnModuleBuilding(InteractionService commandService, ModuleInfo module)
        {
        }

        public virtual void Construct(ModuleBuilder builder, InteractionService commandService)
        {
        }

        internal void SetContext(IInteractionContext context)
        {
            T val = context as T;
            if (val == null)
            {
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
                defaultInterpolatedStringHandler.AppendLiteral("Invalid context type. Expected ");
                defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
                defaultInterpolatedStringHandler.AppendLiteral(", got ");
                defaultInterpolatedStringHandler.AppendFormatted(context.GetType().Name);
                defaultInterpolatedStringHandler.AppendLiteral(".");
                throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
            }

            Context = val;
        }

        protected virtual async Task DeferAsync(bool ephemeral = false, RequestOptions options = null)
        {
            await Context.Interaction.DeferAsync(ephemeral, options).ConfigureAwait(continueOnCapturedContext: false);
        }

        protected virtual async Task RespondAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
        {
            await Context.Interaction.RespondAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
        }

        protected virtual Task RespondWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.RespondWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }
        protected virtual Task<IUserMessage> FollowupWithFileAsync(SixLabors.ImageSharp.Image image, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return DiscordStream.FollowupWithFileAsync(Context, image, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task RespondWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.RespondWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task RespondWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.RespondWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task RespondWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.RespondWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual async Task<IUserMessage> FollowupAsync(string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, RequestOptions options = null, MessageComponent components = null, Embed embed = null)
        {
            return await Context.Interaction.FollowupAsync(text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options).ConfigureAwait(continueOnCapturedContext: false);
        }

        protected virtual Task<IUserMessage> FollowupWithFileAsync(Stream fileStream, string fileName, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.FollowupWithFileAsync(fileStream, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task<IUserMessage> FollowupWithFileAsync(string filePath, string fileName = null, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.FollowupWithFileAsync(filePath, fileName, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task<IUserMessage> FollowupWithFileAsync(FileAttachment attachment, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.FollowupWithFileAsync(attachment, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual Task<IUserMessage> FollowupWithFilesAsync(IEnumerable<FileAttachment> attachments, string text = null, Embed[] embeds = null, bool isTTS = false, bool ephemeral = false, AllowedMentions allowedMentions = null, MessageComponent components = null, Embed embed = null, RequestOptions options = null)
        {
            return Context.Interaction.FollowupWithFilesAsync(attachments, text, embeds, isTTS, ephemeral, allowedMentions, components, embed, options);
        }

        protected virtual async Task<IUserMessage> ReplyAsync(string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null, AllowedMentions allowedMentions = null, MessageReference messageReference = null, MessageComponent components = null)
        {
            return await Context.Channel.SendMessageAsync(text, isTTS: false, embed, options, allowedMentions, messageReference, components).ConfigureAwait(continueOnCapturedContext: false);
        }

        protected virtual Task<IUserMessage> GetOriginalResponseAsync(RequestOptions options = null)
        {
            return Context.Interaction.GetOriginalResponseAsync(options);
        }

        protected virtual Task<IUserMessage> ModifyOriginalResponseAsync(Action<MessageProperties> func, RequestOptions options = null)
        {
            return Context.Interaction.ModifyOriginalResponseAsync(func, options);
        }

        protected virtual async Task DeleteOriginalResponseAsync()
        {
            await (await Context.Interaction.GetOriginalResponseAsync().ConfigureAwait(continueOnCapturedContext: false)).DeleteAsync().ConfigureAwait(continueOnCapturedContext: false);
        }

        protected virtual async Task RespondWithModalAsync(Modal modal, RequestOptions options = null)
        {
            await Context.Interaction.RespondWithModalAsync(modal);
        }

        protected virtual async Task RespondWithModalAsync<TModal>(string customId, RequestOptions options = null) where TModal : class, IModal
        {
            await Context.Interaction.RespondWithModalAsync<TModal>(customId, options);
        }

        void IInteractionModuleBase.SetContext(IInteractionContext context)
        {
            SetContext(context);
        }
    }
}
