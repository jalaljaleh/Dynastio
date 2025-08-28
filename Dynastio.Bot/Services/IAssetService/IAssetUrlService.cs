using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynastio.Bot
{
    public interface IAssetUrlService
    {
        /// <summary>
        /// Returns a fully qualified URL for the given asset.
        /// </summary>
        /// <param name="type">Which image to fetch.</param>
        /// <param name="useCdn">If true, returns jsDelivr CDN URL; otherwise raw GitHub URL.</param>
        /// <returns>Absolute URL to the asset.</returns>
        string GetUrl(AssetType type, bool useCdn = true);

        /// <summary>
        /// Shortcut indexer: equivalent to GetUrl(type, useCdn: true).
        /// </summary>
        string[] this[params AssetType[] type] { get; }
    }
}
