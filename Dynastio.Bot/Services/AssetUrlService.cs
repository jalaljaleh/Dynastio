using System;
using System.Collections.Generic;
using Dynastio.Bot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynastio.Bot
{
    /// <summary>
    /// Provides URLs for assets stored in the Dynastio.Bot GitHub repository.
    /// You can retrieve URLs by <see cref="AssetType"/> enum or by supplying a raw path string.
    /// </summary>
    public class AssetUrlService : IAssetUrlService
    {
        private readonly IServiceProvider _services;

        /// <summary>
        /// Base URL for raw GitHub content (no CDN).
        /// Initialized from <see cref="ConfigurationService"/>.
        /// </summary>
        private readonly string GitHubRawBase;

        /// <summary>
        /// Base URL for jsDelivr CDN.
        /// Initialized from <see cref="ConfigurationService"/>.
        /// </summary>
        private readonly string JsDelivrCdnBase;

        /// <summary>
        /// Common path prefix inside the repository for all bot assets.
        /// </summary>
        private const string AssetsBase = "Dynastio.Graphic/assets/bot/";

        /// <summary>
        /// Maps each <see cref="AssetType"/> to its relative file name under <see cref="AssetsBase"/>.
        /// </summary>
        private static readonly IReadOnlyDictionary<AssetType, string> _paths =
            new Dictionary<AssetType, string>
            {
                [AssetType.banner_dynastio] = "banner_dynastio.jpg",
                [AssetType.banner_not_found] = "banner_not_found.jpg",
                [AssetType.banner_not_found_gif] = "banner_not_found.gif",
                [AssetType.banner_error] = "banner_error.jpg",
                // add other entries here as you expand
            };

        /// <summary>
        /// Constructs the service, pulling configuration for GitHub and CDN bases.
        /// </summary>
        public AssetUrlService(IServiceProvider services)
        {
            _services = services;
            var config = _services.GetRequiredService<ConfigurationService>();
            GitHubRawBase = config.GitHubRawBase;
            JsDelivrCdnBase = config.JsDelivrCdnBase;
        }

        /// <summary>
        /// Returns the full URL for a given <see cref="AssetType"/>, optionally via CDN.
        /// </summary>
        /// <param name="type">Asset identifier enum.</param>
        /// <param name="useCdn">
        /// If true (default), returns the jsDelivr URL; otherwise returns the raw GitHub URL.
        /// </param>
        /// <returns>Absolute URL pointing to the requested asset.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no path is registered for the specified <paramref name="type"/>.
        /// </exception>
        public string GetUrl(AssetType type, bool useCdn = true)
        {
            if (!_paths.TryGetValue(type, out var relativePath))
                throw new KeyNotFoundException($"No path registered for {type}");

            var baseUrl = useCdn ? JsDelivrCdnBase : GitHubRawBase;
            return $"{baseUrl}{AssetsBase}{relativePath}";
        }

        /// <summary>
        /// Indexer shorthand for <see cref="GetUrl(AssetType, bool)"/> using CDN.
        /// </summary>
        public string[] this[params AssetType[] types] => types.Select(a=> GetUrl(a)).ToArray();
      //  public string this[AssetType type] => GetUrl(type);

        /// <summary>
        /// Returns the full URL for an arbitrary asset file under the common assets folder.
        /// </summary>
        /// <param name="relativeFileName">
        /// The file name or sub-path under <c>Dynastio.Graphic/assets/bot/</c>. 
        /// E.g. <c>"icons/foo.png"</c> or <c>"myimage.jpg"</c>.
        /// </param>
        /// <param name="useCdn">
        /// If true (default), returns the jsDelivr URL; otherwise returns the raw GitHub URL.
        /// </param>
        /// <returns>Absolute URL pointing to the requested file.</returns>
        public string GetUrl(string relativeFileName, bool useCdn = true)
        {
            if (string.IsNullOrWhiteSpace(relativeFileName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(relativeFileName));

            var baseUrl = useCdn ? JsDelivrCdnBase : GitHubRawBase;
            return $"{baseUrl}{AssetsBase}{relativeFileName}";
        }

        /// <summary>
        /// Returns the CDN URL for an arbitrary asset file.
        /// </summary>
        public string GetCdnUrl(string relativeFileName) =>
            GetUrl(relativeFileName, useCdn: true);

        /// <summary>
        /// Returns the raw GitHub URL for an arbitrary asset file.
        /// </summary>
        public string GetRawUrl(string relativeFileName) =>
            GetUrl(relativeFileName, useCdn: false);
    }
}
