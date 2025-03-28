using GHent.Shared;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using HtmlAgilityPack;
using Polly;
using GHent.Data;

namespace Ghent.NHentai
{
    public class NHentaiAlbumDownloader(IEventableProgressReporter progress, HtmlWeb htmlWeb, NHentaiItemDownloader nHentaiItemProcessor, DownloadManager downloadManager) : IRequestDownloader
    {
        private const string ThumbnailNodesXPath = "//div[@class='thumbs']/div[@class='thumb-container']";
        private const string AlbumTitleXPath = "//h1[@class='title']/span[@class='pretty']";
        private const string AnchorNodeXPath = ".//a";
        private const string TagsContainerId = "tags";
        private const string TagContainersXPath = "//span[@class=\"name\"]";
        private readonly Uri SimplyHentaiUrl = new("https://nhentai.net");

        private readonly IAsyncPolicy pollyPolicy = 
            Policy.WrapAsync(
                Policy.Handle<Exception>().WaitAndRetryAsync(3, (_) => TimeSpan.FromMilliseconds(100)),
                Policy.TimeoutAsync(30)
             );
        public async Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            HtmlDocument document = await DownloadDocument(request, cancellationToken);
            string savePath = document.ExtractSavePath(request, AlbumTitleXPath);
            var thumbContainerNodes = document.GetThumbnailNodes(request, ThumbnailNodesXPath);

            if (thumbContainerNodes.Count == 0)
            {
                throw new InvalidDataException("Thumb container nodes empty");
            }

            HtmlNode htmlNode = document.GetElementbyId(TagsContainerId);
            var tagsContainers = htmlNode.SelectNodes(TagContainersXPath);
            if (tagsContainers != null)
            {
                var tags = (from tag in tagsContainers
                            select tag.InnerText).ToList();

                var downloadableItem = downloadManager.GetByUrlOrDefault(request.DownloadPath.ToString());
                if (downloadableItem != null)
                {
                    if (downloadableItem.Tags is null)
                    {
                        downloadableItem.Tags = [.. tags];
                    }
                    else
                    {
                        downloadableItem.Tags.UnionWith(tags);
                    }
                    await downloadManager.SaveChangesAsync();
                }
            }

            HashSet<int> skipFiles = GetFilesToSkip(savePath);

            progress.Reset(thumbContainerNodes.Count);
            for (int fileIndex = 0; fileIndex < thumbContainerNodes.Count; fileIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (ShouldSkipFile(fileIndex, skipFiles))
                {
                    progress.Report(ProgressType.Skipped, amount: 1, reportedItem: $"{savePath} - {fileIndex}");
                    continue;
                }

                var thumbContainerNode = thumbContainerNodes[fileIndex];
                if (thumbContainerNode is null)
                {
                    progress.Report(ProgressType.Failure, amount: 0, reportedItem: $"{savePath} - {fileIndex}", message: "Thumb Container node is null");
                    continue;
                }

                var anchorNode = thumbContainerNode.SelectSingleNode(AnchorNodeXPath);
                if (anchorNode is null)
                {
                    progress.Report(ProgressType.Failure, amount: 0, reportedItem: $"{savePath} - {fileIndex}", message: "Anchor node is null");
                    continue;
                }

                // Get the href attribute of the <a> tag
                var href = anchorNode.GetAttributeValue("href", string.Empty);
                if (string.IsNullOrEmpty(href))
                {
                    progress.Report(ProgressType.Failure, amount: 0, reportedItem: $"{savePath} - {fileIndex}", message: $"Href node is null");
                    continue;
                }
                
                var itemRequest = new Request
                {
                    DownloadPath = GetDownloadPath(href),
                    SavePath = savePath,
                };

                await pollyPolicy.ExecuteAsync(async () =>
                {
                    await nHentaiItemProcessor.Download(itemRequest, cancellationToken);
                });
                
            }

            return savePath;
        }

        private static bool ShouldSkipFile(int fileIndex, HashSet<int> skipFiles)
        {
            return skipFiles.Contains(fileIndex);
        }

        private static HashSet<int> GetFilesToSkip(string savePath)
        {
            var files = Directory.GetFiles(savePath);
            var skipFiles = new HashSet<int>();
            foreach (var file in files)
            {
                var lastDashIndex = file.LastIndexOf('\\');
                if (lastDashIndex == -1) continue;

                var dotIndex = file.LastIndexOf('.');

                var numberStr = file[(lastDashIndex + 1)..dotIndex];
                if (int.TryParse(numberStr, out int number))
                {
                    skipFiles.Add(number - 1);
                }
            }

            return skipFiles;
        }

        private async Task<HtmlDocument> DownloadDocument(IRequest request, CancellationToken cancellationToken) => await htmlWeb.LoadFromWebAsync(request.DownloadPath.ToString(), cancellationToken);

        private Uri GetDownloadPath(string href) => new(SimplyHentaiUrl, href);

    }
}