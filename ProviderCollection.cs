using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteEditorCS
{
    /// <summary>
    /// A class to store many provider details as item.
    /// </summary>
    public class ProviderCollection : ObservableCollection<ProviderItem>
    {
        private readonly Dictionary<string, ProviderItem> _byKey;

        public ProviderCollection (IEnumerable<ProviderItem> items)
            : base(items)
        {
            _byKey = this.ToDictionary(p => p.Key, StringComparer.OrdinalIgnoreCase);
        }

        public ProviderItem this[string key]
        {
            get => _byKey[key];
        }

        public bool TryGet (string key, out ProviderItem item) =>
            _byKey.TryGetValue(key ?? string.Empty, out item);
    }
}
