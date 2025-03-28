using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHent.Data
{

    public class DownloadManager : IDownloadableItemsProvider
    {
        private DownloadManager(string filePath)
        {
            _filePath = filePath;
        }

        public static async Task<DownloadManager> CreateAsync(string filePath)
        {
            var manager = new DownloadManager(filePath);
            await manager.LoadItemsAsync();
            return manager;
        }

        public IReadOnlyCollection<DownloadableItem> Items { get => _items.AsReadOnly(); }
        private readonly List<DownloadableItem> _items = [];
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        private readonly string _filePath;

        // Add a new item to the list
        public void AddItem(DownloadableItem item)
        {
            _items.Add(item);
        }

        // Load items from the JSON file asynchronously
        private async Task LoadItemsAsync()
        {
            if (!File.Exists(_filePath))
            {
                // there's nothing to load
                return;
            }

            string json = await File.ReadAllTextAsync(_filePath);

            var items = JsonSerializer.Deserialize<List<DownloadableItem>>(json, _jsonSerializerOptions);

            if (items != null)
            {
                _items.AddRange(items);
            }
        }

        // Save items to the JSON file
        public async Task SaveChangesAsync()
        {
            string json = JsonSerializer.Serialize(Items, _jsonSerializerOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
