using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHent.Data
{
    public class DownloadManager
    {
        public DownloadManager(string filePath)
        {
            this.filePath = filePath;

            LoadItems();
        }
        public IReadOnlyCollection<DownloadableItem> Items { get=>_items.AsReadOnly(); }
        private readonly List<DownloadableItem> _items = [];
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        private readonly string filePath;

        // Add a new item to the list
        public void AddItem(DownloadableItem item)
        {
            _items.Add(item);
        }

        // Load items from the JSON file
        private void LoadItems()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                var items = JsonSerializer.Deserialize<List<DownloadableItem>>(json, _jsonSerializerOptions);

                if (items != null)
                {
                    _items.AddRange(items);
                }
            }
        }

        // Save items to the JSON file
        public async Task SaveChanges()
        {
            string json = JsonSerializer.Serialize(Items, _jsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, json);
        }
    }
}
