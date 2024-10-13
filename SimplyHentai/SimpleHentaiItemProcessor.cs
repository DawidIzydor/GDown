using GHent.Shared.Request;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ghent.SimplyHentai
{
    public class SimpleHentaiItemProcessor(HtmlWeb htmlWeb) : IRequestProcessor
    {
        public async Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            HtmlDocument document = await DownloadDocument(request, cancellationToken);

            string imageUrl = GetImageUrl(request, document);

            // Determine file name from the URL and save path
            var imageFileName = Path.GetFileName(imageUrl);
            var savePath = Path.Combine(request.SavePath, imageFileName);

            await SaveImage(imageUrl, savePath, cancellationToken);

            return savePath;
        }

        private static async Task SaveImage(string imageUrl, string savePath, CancellationToken cancellationToken)
        {
            // Download the image
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(imageUrl, cancellationToken);
            response.EnsureSuccessStatusCode();
            var imageBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            await File.WriteAllBytesAsync(savePath, imageBytes, cancellationToken);
        }

        private Task<HtmlDocument> DownloadDocument(IRequest request, CancellationToken cancellationToken) => htmlWeb.LoadFromWebAsync(request.DownloadPath.ToString(), cancellationToken);

        private static string GetImageUrl(IRequest request, HtmlDocument document)
        {
            HtmlNode imageNode = GetImageNode(request, document);

            // Extract the image URL from the 'src' attribute
            string imageUrl = imageNode.GetAttributeValue("src", null);
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new ArgumentNullException($"Image URL not found: {request.DownloadPath}");
            }

            return imageUrl;
        }

        private static HtmlNode GetImageNode(IRequest request, HtmlDocument document)
        {
            // Get the image node
            var imageNode = document.DocumentNode.SelectSingleNode("//section[@id='image-container']//a//img");
            return imageNode is null ? throw new ArgumentNullException($"Image node not found: {request.DownloadPath}") : imageNode;
        }
    }
}
