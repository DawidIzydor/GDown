using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHent.Data
{
    public class DownloadManager(string filePath)
    {
        public List<DownloadableItem> Items { get; } = [];
        private readonly JsonSerializerOptions _jsonSerializerOptions = new()
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
        };

        // Add a new item to the list
        public void AddItem(DownloadableItem item)
        {
            Items.Add(item);
        }

        // Load items from the JSON file
        public async Task LoadItems()
        {
            Items.Clear();
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
