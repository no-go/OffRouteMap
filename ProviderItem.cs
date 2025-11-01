using GMap.NET.MapProviders;

namespace OffRouteMap
{
    /// <summary>
    /// A class to store details of a map provider.
    /// </summary>
    public sealed class ProviderItem
    {
        public string Key { get; }
        public string DisplayName { get; }
        public GMapProvider Provider { get; }

        public FileCacheProvider FileCache { get; }

        public ProviderItem (string key, string displayName, GMapProvider provider, string cacheRoot)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            DisplayName = displayName ?? key;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            FileCache = new FileCacheProvider(System.IO.Path.Combine(cacheRoot, Key));
        }
    }
}
