using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHent.Data
{

    public class DownloadManager : IDownloadableItemsProvider
    {
        private readonly IFileService _fileService;
        private DownloadManager(string filePath, IFileService fileService)
        {
            _filePath = filePath;
            _fileService = fileService;
        }

        public static async Task<DownloadManager> CreateAsync(string filePath, IFileService fileService)
        {
            var manager = new DownloadManager(filePath, fileService);
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

        public void AddItem(DownloadableItem item)
        {
            _items.Add(item);
        }

        private async Task LoadItemsAsync()
        {
            if (!_fileService.Exists(_filePath))
            {
                return;
            }

            string json = await _fileService.ReadAllTextAsync(_filePath);

            var items = JsonSerializer.Deserialize<List<DownloadableItem>>(json, _jsonSerializerOptions);

            if (items != null)
            {
                _items.AddRange(items);
            }
        }

        public async Task SaveChangesAsync()
        {
            string json = JsonSerializer.Serialize(Items, _jsonSerializerOptions);
            await _fileService.WriteAllTextAsync(_filePath, json);
        }
    }
}
