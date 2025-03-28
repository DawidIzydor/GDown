using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GHent.Data;
using Moq;
using Xunit;

namespace GHent.Tests
{
    public class DownloadManagerTests
    {
        private readonly Mock<IFileService> _fileServiceMock;
        private readonly string _filePath = "test.json";

        public DownloadManagerTests()
        {
            _fileServiceMock = new Mock<IFileService>();
        }

        [Fact]
        public async Task CreateAsync_FileDoesNotExist_ReturnsEmptyDownloadManager()
        {
            // Arrange
            _fileServiceMock.Setup(fs => fs.Exists(_filePath)).Returns(false);

            // Act
            var manager = await DownloadManager.CreateAsync(_filePath, _fileServiceMock.Object);

            // Assert
            Assert.Empty(manager.Items);
        }

        [Fact]
        public async Task CreateAsync_FileExists_LoadsItems()
        {
            // Arrange
            var items = new List<DownloadableItem>
            {
                new() { Url = "http://example.com", Status = DownloadStatus.NotStarted, SavePath = "path1", SaveCbr = false },
                new() { Url = "http://example2.com", Status = DownloadStatus.Queued, SavePath = "path2", SaveCbr = true }
            };
            var json = JsonSerializer.Serialize(items);
            _fileServiceMock.Setup(fs => fs.Exists(_filePath)).Returns(true);
            _fileServiceMock.Setup(fs => fs.ReadAllTextAsync(_filePath)).ReturnsAsync(json);

            // Act
            var manager = await DownloadManager.CreateAsync(_filePath, _fileServiceMock.Object);

            // Assert
            Assert.Equal(2, manager.Items.Count);
        }

        [Fact]
        public async Task AddItem_AddsItemToList()
        {
            // Arrange
            var manager = await DownloadManager.CreateAsync(_filePath, _fileServiceMock.Object);
            var item = new DownloadableItem { Url = "http://example.com", Status = DownloadStatus.NotStarted, SavePath = "path1", SaveCbr = false };

            // Act
            manager.AddItem(item);

            // Assert
            Assert.Contains(item, manager.Items);
        }

        [Fact]
        public async Task SaveChangesAsync_SavesItemsToFile()
        {
            // Arrange
            var manager = await DownloadManager.CreateAsync(_filePath, _fileServiceMock.Object);
            var item = new DownloadableItem { Url = "http://example.com", Status = DownloadStatus.NotStarted, SavePath = "path1", SaveCbr = false };
            manager.AddItem(item);
            string expectedJson = JsonSerializer.Serialize(manager.Items, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
            });

            // Act
            await manager.SaveChangesAsync();

            // Assert
            _fileServiceMock.Verify(fs => fs.WriteAllTextAsync(_filePath, expectedJson), Times.Once);
        }
    }
}
