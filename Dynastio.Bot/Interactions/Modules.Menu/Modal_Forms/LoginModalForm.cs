using Discord;
using Discord.Interactions;

namespace Dynastio.Bot.Interactions.Modules.Menu
{
    /// <summary>
    /// Modal form for users to securely log into their account.
    /// </summary>
    public class AccountLoginModal : IModal
    {
        /// <inheritdoc/>
        public string Title => "Account Login";

        /// <summary>
        /// The unique account identifier (e.g. numeric or alphanumeric string).
        /// </summary>
        [InputLabel( "Account ID")]
        [RequiredInput(true)]
        [ModalTextInput(
            customId: "account_id",
            style: TextInputStyle.Short,
            placeholder: "google:12345678901234567890",
            minLength: 5,
            maxLength: 150)]
        public string AccountId { get; set; }

        /// <summary>
        /// A 4–6 digit personal identification number for added security.
        /// </summary>
        [InputLabel("Secure PIN-Code")]
        [RequiredInput(true)]
        [ModalTextInput(
            customId: "account_pin",
            style: TextInputStyle.Short,
            placeholder: "123-123-123",
            minLength: 11,
            maxLength: 11)]
        public string Pin { get; set; }

        /// <summary>
        /// The name or nickname displayed in the interface.
        /// </summary>
        [InputLabel("Display Name")]
        [RequiredInput(true)]
        [ModalTextInput(
            customId: "display_name",
            style: TextInputStyle.Short,
            placeholder: "YourNickname",
            minLength: 1,
            maxLength: 32)]
        public string DisplayName { get; set; }

        /// <summary>
        /// Contact email address for notifications and recovery.
        /// </summary>
        [InputLabel("Email Address")]
        [RequiredInput(false)]
        [ModalTextInput(
            customId: "email_address",
            style: TextInputStyle.Short,
            placeholder: "user@example.com",
            minLength: 6,
            maxLength: 100)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// Optional field for any additional notes or comments.
        /// </summary>
        [InputLabel("Additional Notes")]
        [RequiredInput(false)]
        [ModalTextInput(
            customId: "notes",
            style: TextInputStyle.Paragraph,
            placeholder: "Any extra info…",
            minLength: 0,
            maxLength: 200)]
        public string Notes { get; set; }
    }
}
