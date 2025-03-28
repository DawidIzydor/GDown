using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GHent.Shared.Request;
using HtmlAgilityPack;
using Moq;
using Xunit;
using GHent.Shared;

namespace GHent.Tests
{
    public class HtmlDocumentHelperTests
    {
        private readonly Mock<IRequest> _mockRequest;
        private readonly HtmlDocument _document;

        public HtmlDocumentHelperTests()
        {
            _mockRequest = new Mock<IRequest>();
            _document = new HtmlDocument();
        }

        [Fact]
        public void ExtractSavePath_ShouldReturnCorrectSavePath()
        {
            // Arrange
            _mockRequest.Setup(r => r.SavePath).Returns("C:\\Downloads");
            _document.LoadHtml("<html><body><div id='title'>AlbumTitle</div></body></html>");

            // Act
            var result = _document.ExtractSavePath(_mockRequest.Object, "//div[@id='title']");

            // Assert
            Assert.Equal("C:\\Downloads\\AlbumTitle", result);
        }

        [Fact]
        public void GetAlbumTitle_ShouldReturnCorrectTitle()
        {
            // Arrange
            _document.LoadHtml("<html><body><div id='title'>AlbumTitle</div></body></html>");

            // Act
            var result = _document.GetAlbumTitle("//div[@id='title']");

            // Assert
            Assert.Equal("AlbumTitle", result);
        }

        [Fact]
        public void GetAlbumTitle_ShouldThrowInvalidOperationException_WhenTitleNotFound()
        {
            // Arrange
            _document.LoadHtml("<html><body></body></html>");

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _document.GetAlbumTitle("//div[@id='title']"));
        }

        [Fact]
        public void GetThumbnailNodes_ShouldReturnCorrectNodes()
        {
            // Arrange
            _mockRequest.Setup(r => r.DownloadPath).Returns(new Uri("http://example.com"));
            _document.LoadHtml("<html><body><div class='thumb'>Thumb1</div><div class='thumb'>Thumb2</div></body></html>");

            // Act
            var result = _document.GetThumbnailNodes(_mockRequest.Object, "//div[@class='thumb']");

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetThumbnailNodes_ShouldThrowArgumentNullException_WhenNodesNotFound()
        {
            // Arrange
            _mockRequest.Setup(r => r.DownloadPath).Returns(new Uri("http://example.com"));
            _document.LoadHtml("<html><body></body></html>");

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _document.GetThumbnailNodes(_mockRequest.Object, "//div[@class='thumb']"));
        }
    }
}
