using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Dynastio.Bot.Database
{
    /// <summary>
    /// Represents a bot user, holding linked game accounts and guild progress.
    /// Sealed to prevent inheritance and preserve internal invariants.
    /// </summary>
    [BsonIgnoreExtraElements]
    public sealed class User
    {
        // --------------------------------------------------------------------
        // Identity and metadata
        // --------------------------------------------------------------------

        /// <summary>
        /// Unique identifier of the user.
        /// Stored in MongoDB as a string (_id field).
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.String)]
        [BsonElement("_id")]
        [JsonInclude, JsonPropertyName("_id")]
        public ulong Id { get; init; }

        /// <summary>
        /// YouTube channel URL or handle associated with this user.
        [JsonInclude, JsonPropertyName("youtubeChannel")]
        /// </summary>
        [BsonElement("youtubeChannel")]
        public string YouTubeChannel { get; private set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when this document was last modified.
        /// Ignored by MongoDB storage (BsonIgnore).
        /// </summary>
        [BsonIgnore, BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime LastUpdatedUtc { get; private set; } = DateTime.UtcNow;

        /// <summary>
        /// All game accounts linked to this user.
        /// </summary>
        [JsonInclude, JsonPropertyName("accounts")]
        [BsonElement("accounts")]
        public List<GameAccount> Accounts { get; private set; } = new();

        /// <summary>
        /// Progress profiles for each guild the user participates in.
        /// </summary>
        [JsonInclude, JsonPropertyName("guildProfiles")]
        [BsonElement("guildProfiles")]
        public List<GuildProgress> GuildProfiles { get; private set; } = new();

        // --------------------------------------------------------------------
        // Convenience Properties
        // --------------------------------------------------------------------

        /// <summary>
        /// Returns true if the user has at least one reward game account.
        /// </summary>
        [BsonIgnore]
        [JsonIgnore]
        public bool HasRewardAccount => Accounts.Any(a => a.IsPrimaryRewardAccount);

        /// <summary>
        /// Returns true if the user has at least one linked game account.
        /// </summary>
        [BsonIgnore]
        [JsonIgnore]
        public bool HasLinkedAccount => Accounts.Count > 0;

        // --------------------------------------------------------------------
        // YouTube Channel
        // --------------------------------------------------------------------

        /// <summary>
        /// Updates the stored YouTube channel, trimming whitespace.
        /// </summary>
        /// <param name="channel">New channel string (URL or handle).</param>
        /// <returns>True if the channel was changed; otherwise false.</returns>
        public bool SetYouTubeChannel(string? channel)
        {
            var next = string.IsNullOrWhiteSpace(channel)
                        ? string.Empty
                        : channel.Trim();

            if (string.Equals(YouTubeChannel, next, StringComparison.Ordinal))
                return false;

            YouTubeChannel = next;
            return true;
        }

        // --------------------------------------------------------------------
        // GameAccount Management
        // --------------------------------------------------------------------

        /// <summary>
        /// Finds a linked account by its service-specific ID.
        /// </summary>
        /// <param name="accountId">The external account identifier.</param>
        /// <returns>The matching GameAccount, or null if none found.</returns>
        public GameAccount? GetAccount(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
                return null;

            return Accounts.FirstOrDefault(a => a.Id == accountId);
        }

        /// <summary>
        /// Checks whether a specific account ID is already linked.
        /// </summary>
        /// <param name="accountId">The external account identifier.</param>
        /// <returns>True if found; otherwise false.</returns>
        public bool HasAccount(string accountId)
        {
            if (string.IsNullOrEmpty(accountId))
                return false;

            return Accounts.Any(a => a.Id == accountId);
        }

        /// <summary>
        /// Links a new game account if it isn’t already present.
        /// </summary>
        /// <param name="account">The GameAccount to add.</param>
        /// <returns>The existing or newly added GameAccount.</returns>
        /// <exception cref="ArgumentNullException">Thrown if account is null.</exception>
        public GameAccount AddAccount(GameAccount account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            var existing = GetAccount(account.Id);
            if (existing != null)
                return existing;

            Accounts.Add(account);
            return account;
        }

        /// <summary>
        /// Inserts or updates a game account entry.
        /// </summary>
        /// <param name="account">The GameAccount to upsert.</param>
        /// <returns>True if the list was modified; otherwise false.</returns>
        /// <exception cref="ArgumentNullException">Thrown if account is null.</exception>
        public bool UpsertAccount(GameAccount account)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));

            for (int i = 0; i < Accounts.Count; i++)
            {
                if (Accounts[i].Id == account.Id)
                {
                    if (!ReferenceEquals(Accounts[i], account))
                    {
                        Accounts[i] = account;
                        return true;
                    }
                    return false;
                }
            }

            Accounts.Add(account);
            return true;
        }

        /// <summary>
        /// Removes a linked account by its ID.
        /// </summary>
        /// <param name="accountId">The external account identifier.</param>
        /// <returns>True if removed; otherwise false.</returns>
        public bool RemoveAccount(string accountId)
        {
            var account = GetAccount(accountId);
            return account != null && Accounts.Remove(account);
        }

        /// <summary>
        /// Marks one account as default and unmarks any other account.
        /// </summary>
        /// <param name="accountId">ID of the account to set as default.</param>
        /// <returns>The newly default GameAccount, or null if not found.</returns>
        public GameAccount? SetDefaultAccount(string accountId)
        {
            var target = GetAccount(accountId);
            if (target == null) return null;

            foreach (var a in Accounts)
                a.AsDefault(a == target);

            return target;
        }
        /// <summary>
        /// Marks one account as Reward if no any mark account available.
        /// </summary>
        /// <param name="accountId">ID of the account to set as reward.</param>
        /// <returns>The newly false, if not found or there be another reward account.</returns>
        public bool SetRewardAccount(string accountId)
        {
            var target = GetAccount(accountId);
            if (target == null) return false;

            if (Accounts.Any(a => a.IsPrimaryRewardAccount))
                return false;

            target.AsPrimaryRewardAccount();
            return true;
        }
        /// <summary>
        /// Retrieves the default account, or first account if none set.
        /// </summary>
        /// <returns>The default GameAccount, or null if no accounts exist.</returns>
        public GameAccount? GetDefaultAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsDefault)
                   ?? Accounts.FirstOrDefault();
        }
        /// <summary>
        /// Retrieves the default account, or first account if none set.
        /// </summary>
        /// <returns>The default GameAccount, or null if no accounts exist.</returns>
        public GameAccount? GetRewardAccount()
        {
            return Accounts.FirstOrDefault(a => a.IsPrimaryRewardAccount);
        }
        /// <summary>
        /// Clears all linked game accounts.
        /// </summary>
        /// <returns>True if any accounts were removed; otherwise false.</returns>
        public bool ClearAccounts()
        {
            if (Accounts.Count == 0) return false;
            Accounts.Clear();
            return true;
        }

        /// <summary>
        /// Returns a distinct set of service names from all linked accounts.
        /// </summary>
        /// <returns>Read-only collection of non-empty service names.</returns>
        public IReadOnlyCollection<string> GetAllServiceNames()
        {
            return Accounts
                .Where(a => !string.IsNullOrEmpty(a.ServiceName))
                .Select(a => a.ServiceName)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        // --------------------------------------------------------------------
        // GuildProgress Management
        // --------------------------------------------------------------------

        /// <summary>
        /// Retrieves an existing guild profile or creates a new one.
        /// </summary>
        /// <param name="guildId">The guild’s unique identifier.</param>
        /// <returns>The existing or newly created GuildProgress.</returns>
        public GuildProgress GetOrCreateGuildProfile(ulong guildId)
        {
            var profile = GuildProfiles.FirstOrDefault(p => p.GuildId == guildId);
            if (profile != null)
                return profile;

            profile = new GuildProgress(guildId);
            GuildProfiles.Add(profile);
            return profile;
        }

        /// <summary>
        /// Attempts to find a guild profile by its guild ID.
        /// </summary>
        /// <param name="guildId">The guild’s unique identifier.</param>
        /// <returns>The matching GuildProgress, or null if none exists.</returns>
        public GuildProgress? TryGetGuildProfile(ulong guildId)
        {
            return GuildProfiles.FirstOrDefault(p => p.GuildId == guildId);
        }

        /// <summary>
        /// Removes a guild profile by its ID.
        /// </summary>
        /// <param name="guildId">The guild’s unique identifier.</param>
        /// <returns>True if removed; otherwise false.</returns>
        public bool RemoveGuildProfile(ulong guildId)
        {
            var profile = TryGetGuildProfile(guildId);
            return profile != null && GuildProfiles.Remove(profile);
        }

        /// <summary>
        /// Clears all guild progress profiles.
        /// </summary>
        /// <returns>True if any profiles were removed; otherwise false.</returns>
        public bool ClearGuildProfiles()
        {
            if (GuildProfiles.Count == 0) return false;
            GuildProfiles.Clear();
            return true;
        }

        /// <summary>
        /// Enumerates all guild progress entries.
        /// </summary>
        /// <returns>IEnumerable of GuildProgress.</returns>
        public IEnumerable<GuildProgress> GetAllGuildProfiles() => GuildProfiles;

        // --------------------------------------------------------------------
        // Validation and maintenance
        // --------------------------------------------------------------------

        /// <summary>
        /// Ensures that the collections are non-null (after deserialization).
        /// </summary>
        public void EnsureDefaults()
        {
            Accounts ??= new List<GameAccount>();
            GuildProfiles ??= new List<GuildProgress>();
        }

        /// <summary>
        /// Performs basic validation of the user instance.
        /// </summary>
        /// <param name="error">Error message if validation fails.</param>
        /// <returns>True if valid; otherwise false.</returns>
        public bool TryValidate(out string? error)
        {
            if (Id == 0)
            {
                error = "User Id must be non-zero.";
                return false;
            }

            if (Accounts == null)
            {
                error = "Accounts list must not be null.";
                return false;
            }

            if (GuildProfiles == null)
            {
                error = "GuildProfiles list must not be null.";
                return false;
            }

            error = null;
            return true;
        }

        // --------------------------------------------------------------------
        // Internals
        // --------------------------------------------------------------------

        /// <summary>
        /// Updates the LastUpdatedUtc timestamp to now.
        /// </summary>
        public void Touch() => LastUpdatedUtc = DateTime.UtcNow;

        /// <summary>
        /// Returns a human-readable representation of this user.
        /// </summary>
        public override string ToString() =>
            $"BotUser(Id={Id}, Accounts={Accounts.Count}, Guilds={GuildProfiles.Count}, Updated={LastUpdatedUtc:O})";
    }
}
