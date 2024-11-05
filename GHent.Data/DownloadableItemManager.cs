using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHent.Data
{
    public class DownloadManager(string filePath)
    {
        public List<DownloadableItem> Items { get; private set; }
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };
        public async ValueTask<DownloadableItem?> GetItemOrDefault(string url)
        {
            if(Items is null)
            {
                await LoadItems();
            }

            return Items.FirstOrDefault(i => i.Url == url);
        }

        // Add a new item to the list
        public void AddItem(DownloadableItem item)
        {
            Items.Add(item);
        }

        // Remove an item from the list
        public void RemoveItem(string url)
        {
            var item = Items.FirstOrDefault(i => i.Url == url);
            if (item is not null)
            {
                Items.Remove(item);
            }
        }

        // Load items from the JSON file
        public async Task LoadItems()
        {
            Items = new List<DownloadableItem>();
            if (File.Exists(filePath))
            {
                string json = await File.ReadAllTextAsync(filePath);

                var items = JsonSerializer.Deserialize<List<DownloadableItem>>(json, _jsonSerializerOptions);

                if (items != null)
                {
                    Items.AddRange(items);
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
