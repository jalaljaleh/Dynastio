using Discord;
using Discord.WebSocket;
using Dynastio.Bot.Global;
using Dynastio.Graphic;
using Dynastio.Net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Dynastio.Bot.Services
{

    public class EmoteService
    {
        private readonly DiscordSocketClient _discord;

        // Cache
        private readonly SemaphoreSlim _cacheLock = new(1, 1);
        private Dictionary<string, Emote> _emotesByName = new(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _emoteNames = new(StringComparer.OrdinalIgnoreCase);
        private volatile bool _ready;

        // Defaults and unknown fallback
        private static readonly TimeSpan DefaultDelay = TimeSpan.FromMilliseconds(900);
        private string _unknownEmoteName = "unknown";
        private string _unknownEmoteFilePath = DynastioGraphicHelper.GetUnkownPng(); // optional: set if you want auto-create

        public EmoteService(IServiceProvider services)
        {
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _discord.Ready += OnDiscordReadyAsync;
        }

        // Configure the unknown emote name (and optional file path to auto-create if missing)
        public void SetUnknownEmoteName(string name) => _unknownEmoteName = name ?? "unknown";
        public void SetUnknownEmoteFilePath(string filePath) => _unknownEmoteFilePath = filePath;

        // Called by Discord when the socket is ready
        private async Task OnDiscordReadyAsync()
        {
            await RefreshCacheAsync().ConfigureAwait(false);
            _ready = true;
            Console.WriteLine("Emotes Service    Cache primed with application emotes.");

            // Optional: ensure the unknown emote exists
            if (!string.IsNullOrWhiteSpace(_unknownEmoteFilePath))
                await EnsureUnknownEmotePresentAsync().ConfigureAwait(false);

            await SyncAllAsync();
        }

        // Optional manual warm-up (e.g., in your startup after login)
        public async Task EnsureReadyAsync(CancellationToken ct = default)
        {
            if (_ready) return;
            for (int i = 0; i < 20 && !_ready; i++)
                await Task.Delay(100, ct).ConfigureAwait(false);
            if (!_ready)
                await RefreshCacheAsync(ct).ConfigureAwait(false);
        }

        // Refresh in-memory cache of application emotes
        public async Task RefreshCacheAsync(CancellationToken ct = default)
        {
            await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                Console.WriteLine("Emotes Service    Getting discord application images...");
                var emotes = await _discord.GetApplicationEmotesAsync().ConfigureAwait(false);

                var byName = new Dictionary<string, Emote>(emotes.Count, StringComparer.OrdinalIgnoreCase);
                foreach (var e in emotes)
                    byName[e.Name] = e;

                _emotesByName = byName;
                _emoteNames = new HashSet<string>(_emotesByName.Keys, StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        // Ensure the unknown emote exists; upload if missing (requires _unknownEmoteFilePath)
        public async Task EnsureUnknownEmotePresentAsync(CancellationToken ct = default)
        {
            await EnsureReadyAsync(ct).ConfigureAwait(false);

            bool hasUnknown;
            await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                hasUnknown = _emoteNames.Contains(_unknownEmoteName);
            }
            finally
            {
                _cacheLock.Release();
            }

            if (!hasUnknown && !string.IsNullOrWhiteSpace(_unknownEmoteFilePath) && File.Exists(_unknownEmoteFilePath))
            {
                await CreateEmoteFromFileAsync(_unknownEmoteName, _unknownEmoteFilePath, ct).ConfigureAwait(false);
                await RefreshCacheAsync(ct).ConfigureAwait(false);
                Console.WriteLine($"Emotes Service    Unknown emote '{_unknownEmoteName}' created.");
            }
        }

        // Name normalization: tweak if you need other rules
        private static string NormalizeName(string name) => name.ToLower().Trim();

        // Fallback resolver
        private Emote GetUnknownOrDefault()
            => _emotesByName.TryGetValue(_unknownEmoteName, out var unk) ? unk : default;

        // Lookups with fallback
        public Emote GetEmote(ItemType item)
            => GetEmoteByName(item.ToString().ToLowerInvariant());

        public Emote GetEmote(EntityType entity)
            => GetEmoteByName(entity.ToString().ToLowerInvariant());

        // If you have a BadgeType enum; adjust the name if your enum differs
        public Emote GetEmote(BadgeType badge)
            => GetEmoteByName(badge.ToString().ToLowerInvariant());

        // Works for any enum when the name matches your emote keys
        public string GetEmoteTag<TEnum>(TEnum value)
            where TEnum : struct, Enum
        {
            var name = value.ToString().ToLowerInvariant();
            var emote = GetEmoteByName(name);
            return emote.ToDiscordTag();
        }
        public Emote GetEmoteByName(string name)
        {
            var key = NormalizeName(name);
            if (_emotesByName.TryGetValue(key, out var e))
                return e;

            // Fallback to unknown emote
            return GetUnknownOrDefault();
        }

        // Create a single emote (file path). Stream version tends to be friendlier memory-wise.
        public async Task CreateEmoteFromFileAsync(string name, string filePath, CancellationToken ct = default)
        {
            var normalized = NormalizeName(name);

            // Stream approach (optional):
            // await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            // await _discord.CreateApplicationEmoteAsync(normalized, new Image(fs)).ConfigureAwait(false);

            await _discord.CreateApplicationEmoteAsync(normalized, new Image(filePath)).ConfigureAwait(false);

            await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                // We don’t have the Emote instance here; refresh later if you need details.
                _emoteNames.Add(normalized);
            }
            finally
            {
                _cacheLock.Release();
            }
        }


        // Public one-call sync for items and/or entities
        public async Task<(int itemsUploaded, int entitiesUploaded)> SyncAllAsync(
            bool includeUi = true,
            bool includeItems = true,
            bool includeEntities = true,
            bool includeBadges = true,
            bool includeSkins = true,
            bool dryRun = false,
            int? maxUploadsPerCategory = null,
            TimeSpan? delayBetweenUploads = null,
            Func<string, bool> fileNameFilter = null,
            Action<string> logger = null,
            CancellationToken ct = default)
        {
            await EnsureReadyAsync(ct).ConfigureAwait(false);

            var delay = delayBetweenUploads ?? DefaultDelay;
            logger ??= msg => Console.WriteLine($"Application Emotes    {msg}");

            // Ensure unknown exists (optional)
            if (!string.IsNullOrWhiteSpace(_unknownEmoteFilePath))
                await EnsureUnknownEmotePresentAsync(ct).ConfigureAwait(false);

            int items = 0, entities = 0, ui = 0, skins = 0, badges = 0;
            if (includeUi)
            {
                var itemPaths = DynastioGraphicHelper.GetUisPng();
                ui = await SyncFromPathsAsync(
                    kind: "Ui",
                    filePaths: itemPaths,
                    dryRun: dryRun,
                    maxUploads: maxUploadsPerCategory,
                    delayBetweenUploads: delay,
                    fileNameFilter: fileNameFilter,
                    logger: logger,
                    ct: ct);
            }
            if (includeItems)
            {
                var itemPaths = DynastioGraphicHelper.GetItemsPng();
                items = await SyncFromPathsAsync(
                    kind: "Items",
                    filePaths: itemPaths,
                    dryRun: dryRun,
                    maxUploads: maxUploadsPerCategory,
                    delayBetweenUploads: delay,
                    fileNameFilter: fileNameFilter,
                    logger: logger,
                    ct: ct);
            }

            if (includeEntities)
            {
                var entityPaths = DynastioGraphicHelper.GetEntitiesPng();
                entities = await SyncFromPathsAsync(
                    kind: "Entities",
                    filePaths: entityPaths,
                    dryRun: dryRun,
                    maxUploads: maxUploadsPerCategory,
                    delayBetweenUploads: delay,
                    fileNameFilter: fileNameFilter,
                    logger: logger,
                    ct: ct);
            }
            if (includeBadges)
            {
                var entityPaths = DynastioGraphicHelper.GetBadgesPng();
                badges = await SyncFromPathsAsync(
                    kind: "Badges",
                    filePaths: entityPaths,
                    dryRun: dryRun,
                    maxUploads: maxUploadsPerCategory,
                    delayBetweenUploads: delay,
                    fileNameFilter: fileNameFilter,
                    logger: logger,
                    ct: ct);
            }
            if (includeSkins)
            {
                var entityPaths = DynastioGraphicHelper.GetSkinPng();
                skins = await SyncFromPathsAsync(
                    kind: "Skins",
                    filePaths: entityPaths,
                    dryRun: dryRun,
                    maxUploads: maxUploadsPerCategory,
                    delayBetweenUploads: delay,
                    fileNameFilter: fileNameFilter,
                    logger: logger,
                    ct: ct);
            }

            if ((items + badges + entities + skins + ui) > 0)
                await RefreshCacheAsync();

            return (items, entities);
        }

        public Task<int> SyncSkinAsync(
         bool dryRun = false,
         int? maxUploads = null,
         TimeSpan? delayBetweenUploads = null,
         Func<string, bool> fileNameFilter = null,
         Action<string> logger = null,
         CancellationToken ct = default)
         => SyncFromPathsAsync("Skins", DynastioGraphicHelper.GetUisPng(), dryRun, maxUploads,
                               delayBetweenUploads ?? DefaultDelay, fileNameFilter, logger, ct);
        public Task<int> SyncUiAsync(
           bool dryRun = false,
           int? maxUploads = null,
           TimeSpan? delayBetweenUploads = null,
           Func<string, bool> fileNameFilter = null,
           Action<string> logger = null,
           CancellationToken ct = default)
           => SyncFromPathsAsync("Ui", DynastioGraphicHelper.GetUisPng(), dryRun, maxUploads,
                                 delayBetweenUploads ?? DefaultDelay, fileNameFilter, logger, ct);
        // Sync for only badges
        public Task<int> SyncBadgesAsync(
            bool dryRun = false,
            int? maxUploads = null,
            TimeSpan? delayBetweenUploads = null,
            Func<string, bool> fileNameFilter = null,
            Action<string> logger = null,
            CancellationToken ct = default)
            => SyncFromPathsAsync("Badges", DynastioGraphicHelper.GetBadgesPng(), dryRun, maxUploads,
                                  delayBetweenUploads ?? DefaultDelay, fileNameFilter, logger, ct);
        public Task<int> SyncItemsAsync(
            bool dryRun = false,
            int? maxUploads = null,
            TimeSpan? delayBetweenUploads = null,
            Func<string, bool> fileNameFilter = null,
            Action<string> logger = null,
            CancellationToken ct = default)
            => SyncFromPathsAsync("Items", DynastioGraphicHelper.GetItemsPng(), dryRun, maxUploads,
                                  delayBetweenUploads ?? DefaultDelay, fileNameFilter, logger, ct);

        public Task<int> SyncEntitiesAsync(
            bool dryRun = false,
            int? maxUploads = null,
            TimeSpan? delayBetweenUploads = null,
            Func<string, bool> fileNameFilter = null,
            Action<string> logger = null,
            CancellationToken ct = default)
            => SyncFromPathsAsync("Entities", DynastioGraphicHelper.GetEntitiesPng(), dryRun, maxUploads,
                                  delayBetweenUploads ?? DefaultDelay, fileNameFilter, logger, ct);

        private async Task<int> SyncFromPathsAsync(
            string kind,
            IEnumerable<string> filePaths,
            bool dryRun,
            int? maxUploads,
            TimeSpan delayBetweenUploads,
            Func<string, bool> fileNameFilter,
            Action<string> logger,
            CancellationToken ct)
        {
            logger ??= msg => Console.WriteLine($"Application Emotes    {msg}");

            await EnsureReadyAsync(ct).ConfigureAwait(false);

            // Snapshot emote names to avoid locking during enumeration
            HashSet<string> existingNames;
            await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                existingNames = new HashSet<string>(_emoteNames, _emoteNames.Comparer);
            }
            finally
            {
                _cacheLock.Release();
            }

            var toAdd = new List<(string Name, string Path)>(capacity: 256);
            int already = 0;

            foreach (var path in filePaths)
            {
                ct.ThrowIfCancellationRequested();

                var name = NormalizeName(Path.GetFileNameWithoutExtension(path));
                if (fileNameFilter != null && !fileNameFilter(name))
                    continue;

                if (existingNames.Contains(name))
                {
                    already++;
                }
                else
                {
                    toAdd.Add((name, path));
                }
            }

            logger($"[{kind}] {already} images already present.");
            logger($"[{kind}] {toAdd.Count} images to add.");

            if (dryRun || toAdd.Count == 0)
            {
                if (dryRun)
                    logger($"[{kind}] Dry-run: no uploads performed.");
                return 0;
            }

            int total = maxUploads.HasValue ? Math.Min(toAdd.Count, maxUploads.Value) : toAdd.Count;
            int uploaded = 0;

            for (int i = 0; i < total; i++)
            {
                ct.ThrowIfCancellationRequested();

                var (name, path) = toAdd[i];

                try
                {
                    // Stream version (optional):
                    // await using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    // await _discord.CreateApplicationEmoteAsync(name, new Image(fs)).ConfigureAwait(false);

                    await _discord.CreateApplicationEmoteAsync(name, new Image(path)).ConfigureAwait(false);
                    uploaded++;

                    // Update in-memory sets incrementally to keep lookups coherent
                    await _cacheLock.WaitAsync(ct).ConfigureAwait(false);
                    try { _emoteNames.Add(name); } finally { _cacheLock.Release(); }

                    logger($"[{kind}] [{i + 1}/{total}] Added {name}");
                }
                catch (Exception ex)
                {
                    logger($"[{kind}] [{i + 1}/{total}] Failed {name}: {ex.Message}");
                }

                if (i < total - 1 && delayBetweenUploads > TimeSpan.Zero)
                {
                    try
                    {
                        await Task.Delay(delayBetweenUploads, ct).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { throw; }
                }
            }

            return uploaded;
        }
    }
}
