using GHent.Models;
using HtmlAgilityPack;
using System.Xml.Linq;


namespace Ghent.SimplyHentai
{
    public class SimplyHentaiAlbumRequestProcessor(IProgress<DownloadProgressReport> progress, HtmlWeb htmlWeb) : IRequestProcessor
    {
        private const string SimplyHentaiUrl = "simplyhentai.org";
        public async Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            var albumRequestSavePath = request.SavePath;
            var document = await htmlWeb.LoadFromWebAsync(request.DownloadPath.ToString(), cancellationToken)
                .ConfigureAwait(false);
            //var pages = ParsePagesCount(document);

            var albumTitle = GetAlbumTitle(document);

            var savePath = Path.Combine(albumRequestSavePath, albumTitle);
            EnsurePathExists(savePath);

            var alreadyDownloaded = FilesCount(savePath);

            var thumbContainerNodes = GetThumbnailNodes(document);

            var taskSet = new HashSet<Task<string>>();

            if (thumbContainerNodes == null)
            {
                throw new InvalidOperationException("Thumb containers nodes null");
            }
            
            var itemProcessor = new SimpleHentaiItemProcessor(htmlWeb);
            for (int fileIndex = alreadyDownloaded; fileIndex < thumbContainerNodes.Count; fileIndex++)
            {
                var thumbContainerNode = thumbContainerNodes[fileIndex];

                if (thumbContainerNode is null) continue;

                var anchorNode = thumbContainerNode.SelectSingleNode(".//a");
                if (anchorNode is null)
                {
                    continue;
                }
                // Get the href attribute of the <a> tag
                var href = anchorNode.GetAttributeValue("href", string.Empty);

                if (!string.IsNullOrEmpty(href))
                {
                    var itemRequest = new Request
                    {
                        DownloadPath = new Uri($"https://{SimplyHentaiUrl}{href}"),
                        SavePath = savePath,
                    };
                    taskSet.Add(itemProcessor.Download(itemRequest, cancellationToken));
                }
            }

            var progressReport = new DownloadProgressReport
            {
                All = taskSet.Count
            };
            while(taskSet.Count > 0)
            {
                var task = await Task.WhenAny(taskSet).ConfigureAwait(false);
                taskSet.Remove(task);

                progressReport.Finished++;
                progressReport.FinishedPath = await task;

                progress.Report(progressReport);

            }


            return savePath;
        }

        private static HtmlNodeCollection GetThumbnailNodes(HtmlDocument document) => document.DocumentNode.SelectNodes("//div[@class='thumbs']/div[@class='thumb-container']");

        private static int FilesCount(string savePath) => Directory.GetFiles(savePath).Length;

        private static void EnsurePathExists(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        private static string GetAlbumTitle(HtmlDocument document)
        {
            var nameNode = document.DocumentNode.SelectSingleNode("//h1[@class='title']/span[@class='pretty']");
            if (nameNode != null)
            {
                return nameNode.InnerHtml.RemoveIllegalCharacters();
                // Do something with the name
            }
            else
            {
                throw new InvalidOperationException("Title not found");
            }
        }
    }
}