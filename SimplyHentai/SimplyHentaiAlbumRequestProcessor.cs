using GHent.Models;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using HtmlAgilityPack;
using System.Xml.Linq;


namespace Ghent.SimplyHentai
{
    public class SimplyHentaiAlbumRequestProcessor(IProgressReporter<string> progress, HtmlWeb htmlWeb, IImageSaver imageSaver) : IRequestProcessor
    {
        private const string ThumbnailNodesXPath = "//div[@class='thumbs']/div[@class='thumb-container']";
        private const string AlbumTitleXPath = "//h1[@class='title']/span[@class='pretty']";
        private const string AnchorNodeXPath = ".//a";
        private readonly Uri SimplyHentaiUrl = new("https://simplyhentai.org");
        public async Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            HtmlDocument document = await DownloadDocument(request, cancellationToken);
            string savePath = GenerateSavePath(request, document);
            var thumbContainerNodes = GetThumbnailNodes(document, request);

            HashSet<Task<string>> taskSet = StartDownloadTasks(thumbContainerNodes, savePath, cancellationToken);

            await ProgressAllTasks(taskSet, progress).ConfigureAwait(false);

            return savePath;
        }

        private static async Task ProgressAllTasks(HashSet<Task<string>> taskSet, IProgressReporter<string> progress)
        {
            progress.Reset(taskSet.Count);
            while (taskSet.Count > 0)
            {
                var task = await Task.WhenAny(taskSet).ConfigureAwait(false);
                taskSet.Remove(task);

                progress.Report(await task);
            }
        }

        private HashSet<Task<string>> StartDownloadTasks(HtmlNodeCollection thumbContainerNodes, string savePath, CancellationToken cancellationToken)
        {
            var alreadyDownloaded = FilesCount(savePath);
            var taskSet = new HashSet<Task<string>>();
            var itemProcessor = new SimplyHentaiItemProcessor(htmlWeb, imageSaver);
            for (int fileIndex = alreadyDownloaded; fileIndex < thumbContainerNodes.Count; fileIndex++)
            {
                var thumbContainerNode = thumbContainerNodes[fileIndex];

                if (thumbContainerNode is null) continue;

                var anchorNode = GetAnchorNode(thumbContainerNode);
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
                        DownloadPath = GetDownloadPath(href),
                        SavePath = savePath,
                    };
                    taskSet.Add(itemProcessor.Download(itemRequest, cancellationToken));
                }
            }

            return taskSet;
        }

        private static HtmlNode GetAnchorNode(HtmlNode thumbContainerNode)
        {
            return thumbContainerNode.SelectSingleNode(AnchorNodeXPath);
        }

        private static string GenerateSavePath(IRequest request, HtmlDocument document)
        {
            var albumTitle = GetAlbumTitle(document);
            var albumRequestSavePath = request.SavePath;
            var savePath = Path.Combine(albumRequestSavePath, albumTitle);
            EnsurePathExists(savePath);
            return savePath;
        }

        private async Task<HtmlDocument> DownloadDocument(IRequest request, CancellationToken cancellationToken) => await htmlWeb.LoadFromWebAsync(request.DownloadPath.ToString(), cancellationToken);

        private Uri GetDownloadPath(string href) => new(SimplyHentaiUrl, href);

        private static HtmlNodeCollection GetThumbnailNodes(HtmlDocument document, IRequest request)
        {
            var thumbContainerNodes = document.DocumentNode.SelectNodes(ThumbnailNodesXPath);
            return thumbContainerNodes is null
                ? throw new ArgumentNullException($"Thumb containers nodes null: {request.DownloadPath}")
                : thumbContainerNodes;
        }

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
            var nameNode = document.DocumentNode.SelectSingleNode(AlbumTitleXPath);
            if (nameNode != null)
            {
                return nameNode.InnerHtml.RemoveIllegalCharacters();
            }
            else
            {
                throw new InvalidOperationException("Title not found");
            }
        }
    }
}