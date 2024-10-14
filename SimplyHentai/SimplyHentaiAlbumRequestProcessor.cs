using GHent.Shared;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using HtmlAgilityPack;
using Polly;
using Polly.Retry;


namespace Ghent.SimplyHentai
{
    public class SimplyHentaiAlbumRequestProcessor(IProgressReporter<ProgressData<string>> progress, HtmlWeb htmlWeb, IImageSaver imageSaver) : IRequestProcessor
    {
        private const string ThumbnailNodesXPath = "//div[@class='thumbs']/div[@class='thumb-container']";
        private const string AlbumTitleXPath = "//h1[@class='title']/span[@class='pretty']";
        private const string AnchorNodeXPath = ".//a";
        private readonly Uri SimplyHentaiUrl = new("https://nhentai.net");

        private readonly AsyncRetryPolicy retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(1, (_) => TimeSpan.FromMilliseconds(100));
        public async Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            HtmlDocument document = await DownloadDocument(request, cancellationToken);
            string savePath = GenerateSavePath(request, document);
            var thumbContainerNodes = GetThumbnailNodes(document, request);

            var itemProcessor = new SimplyHentaiItemProcessor(htmlWeb, imageSaver, progress);

            if (thumbContainerNodes.Count == 0)
            {
                throw new InvalidDataException("Thumb container nodes empty");
            }

            var files = Directory.GetFiles(savePath);
            var skipFiles = new HashSet<int>();
            foreach (var file in files) {
                var lastDashIndex = file.LastIndexOf("\\");
                if (lastDashIndex == -1) continue;

                var dotIndex = file.LastIndexOf(".");

                var numberStr = file[(lastDashIndex+1)..dotIndex];
                int number;
                if (int.TryParse(numberStr, out number))
                {
                    skipFiles.Add(number);
                }
            }

            progress?.Reset(thumbContainerNodes.Count);
            for (int fileIndex = 0; fileIndex < thumbContainerNodes.Count; fileIndex++)
            {
                if (skipFiles.Contains(fileIndex)) {
                    progress?.Report(new ProgressData<string>
                    {
                        Type = ProgressType.Skipped,
                        Value = savePath,
                        Information = fileIndex.ToString()
                    });
                    continue;
                }

                var thumbContainerNode = thumbContainerNodes[fileIndex];

                if (thumbContainerNode is null) {
                    progress?.Report(new ProgressData<string>
                    {
                        Value = $"{savePath} - {fileIndex}",
                        Type = ProgressType.Failure,
                        Information = $"Thumb Container node is null"
                    });
                    continue; 
                }

                var anchorNode = GetAnchorNode(thumbContainerNode);
                if (anchorNode is null)
                {
                    progress?.Report(new ProgressData<string>
                    {
                        Value = $"{savePath} - {fileIndex}",
                        Type = ProgressType.Failure,
                        Information = $"Anchor node is null"
                    });
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

                    await retryPolicy.ExecuteAsync(async () =>
                    {
                        await itemProcessor.Download(itemRequest, cancellationToken);
                    });
                }
                else
                {
                    progress?.Report(new ProgressData<string>
                    {
                        Value = $"{savePath} - {fileIndex}",
                        Type = ProgressType.Failure,
                        Information = $"Href node is null"
                    });
                }
            }

            return savePath;
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
            if(savePath.Length > 128)
            {
                savePath = savePath[0..127];
            }
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