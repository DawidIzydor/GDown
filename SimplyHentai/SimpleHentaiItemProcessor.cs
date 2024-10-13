using GHent.Models;
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

            var document = await htmlWeb.LoadFromWebAsync(request.DownloadPath.ToString(), cancellationToken)
                .ConfigureAwait(false);

            // Get the image node
            var imageNode = document.DocumentNode.SelectSingleNode("//section[@id='image-container']//a//img");
            if (imageNode == null)
            {
                throw new InvalidOperationException($"Image node not found: {request.DownloadPath}");
            }

            // Extract the image URL from the 'src' attribute
            string imageUrl = imageNode.GetAttributeValue("src", null);
            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new InvalidOperationException($"Image URL not found: {request.DownloadPath}");
            }

            // Determine file name from the URL and save path
            var imageFileName = Path.GetFileName(imageUrl);
            var savePath = Path.Combine(request.SavePath, imageFileName);

            // Download the image
            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetAsync(imageUrl, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var imageBytes = await response.Content.ReadAsByteArrayAsync();

                await File.WriteAllBytesAsync(savePath, imageBytes, cancellationToken);
            }

            return savePath;
        }
    }
}
