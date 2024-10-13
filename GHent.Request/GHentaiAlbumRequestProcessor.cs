using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHent.GHentai.Singleton;
using GHent.Models;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace GHent.GHentai
{
    class AlbumInfo
    {
        public string DownloadUrl { get; set; }
    }
    public class GHentaiAlbumRequestProcessor(IProgressReporter<string> progress, HtmlWeb htmlWeb) : IRequestProcessor
    {
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.IOException">The directory specified by <paramref>path</paramref> is a file.   -or-   The network name is not known.</exception>
        /// <exception cref="T:System.OverflowException"><paramref>s</paramref> represents a number less than
        ///     <see cref="F:System.Int32.MinValue"></see> or greater than <see cref="F:System.Int32.MaxValue"></see>.</exception>
        /// <exception cref="T:GHent.RequestProcessor.TransferExceededException">Transfer was exceeded</exception>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:System.AggregateException"></exception>
        /// <exception cref="TransferExceededException">Transfer exceeded</exception>
        public Task<string> Download(IRequest request, CancellationToken cancellationToken)
        {
            try
            {
                return DownloadInternalAsync(request, cancellationToken);
            }
            catch (AggregateException ex)
            {
                if (ex.InnerException is TransferExceededException transferExceededException)
                {
                    throw transferExceededException;
                }

                throw;
            }
        }

        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:GHent.RequestProcessor.TransferExceededException">Transfer was exceeded</exception>
        /// <exception cref="T:System.InvalidOperationException">The local file specified by fileName is in use by another thread.</exception>
        /// <exception cref="T:System.OverflowException"><paramref>s</paramref> represents a number less than
        ///     <see cref="F:System.Int32.MinValue"></see> or greater than <see cref="F:System.Int32.MaxValue"></see>.</exception>
        /// <exception cref="T:System.IO.IOException">The directory specified by <paramref>path</paramref> is a file.   -or-   The network name is not known.</exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="T:System.AggregateException">The task was canceled. The <see cref="P:System.AggregateException.InnerExceptions"></see> collection contains a <see cref="T:System.Threading.Tasks.TaskCanceledException"></see> object.   -or-   An exception was thrown during the execution of the task. The <see cref="P:System.AggregateException.InnerExceptions"></see> collection contains information about the exception or exceptions.</exception>
        private async Task<string> DownloadInternalAsync(IRequest albumRequest, CancellationToken cancellationToken)
        {
            var albumRequestSavePath = albumRequest.SavePath;
            var netPath = albumRequest.DownloadPath;
            var document = await htmlWeb.LoadFromWebAsync(netPath.ToString(), cancellationToken)
                .ConfigureAwait(false);
            var pages = ParsePagesCount(document);

            var name = GetAlbumName(document);

            var savePath = Path.Combine(albumRequestSavePath, name);
            EnsurePathExists(savePath);

            string albumInfoPath = Path.Combine(savePath, "albuminfo.json");
            AlbumInfo albumInfo;
            if (!File.Exists(albumInfoPath))
            {
                albumInfo = new AlbumInfo
                {
                    DownloadUrl = albumRequest.DownloadPath.AbsoluteUri
                };

                var serializedAlbumInfo = JsonConvert.SerializeObject(albumInfo, Formatting.Indented);
                await File.WriteAllTextAsync(albumInfoPath, serializedAlbumInfo, cancellationToken);
            }


            var filesInPath = Directory.GetFiles(savePath).Length;
            var pagesToIgnore = filesInPath / 40;

            var tasks = new List<Task<string>>();
            for (var page = pagesToIgnore; page < pages; page++)
            {
                var htmlDocument = await htmlWeb.LoadFromWebAsync(netPath + "?p=" + page, cancellationToken).ConfigureAwait(false);
                var gdt = htmlDocument.GetElementbyId("gdt");

                // ReSharper disable once StringLiteralTypo
                tasks.AddRange(gdt.ChildNodes.Where(gdtChild => gdtChild.HasClass("gdtm"))
                    .Select(el => StartDownloadingAsync(el, savePath, cancellationToken)));
            }


            progress.Reset(tasks.Count);
         
            while (tasks.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var finishedTask = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(finishedTask);

                progress.Report(await finishedTask);
            }

            return savePath;
        }

        private static string GetAlbumName(HtmlDocument document) => document.GetElementbyId("gn").InnerHtml.RemoveIllegalCharacters();

        private static void EnsurePathExists(string savePath)
        {
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
        }

        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:GHent.RequestProcessor.TransferExceededException">Transfer was exceeded</exception>
        /// <exception cref="T:System.InvalidOperationException">The local file specified by fileName is in use by another thread.</exception>
        /// <exception cref="T:System.Net.WebException">The URI formed by combining
        ///     <see cref="P:System.Net.WebClient.BaseAddress"></see> and address is invalid.   -or-   An error occurred while
        ///     downloading the resource.</exception>
        /// <exception cref="T:System.ArgumentNullException">The >address parameter is null.   -or-   The fileName parameter is
        ///     null.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The associated
        ///     <see cref="T:System.Threading.CancellationTokenSource"></see> has been disposed.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref>path1</paramref> or <paramref>path2</paramref> contains one or
        ///     more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars"></see>.</exception>
        private Task<string> StartDownloadingAsync(HtmlNode el, string savePath,
            CancellationToken cancellationToken)
        {
            var selectSingleNode = el.SelectSingleNode("div//a");
            var uriString = selectSingleNode.Attributes["href"].Value;
            var processor = new ImageRequestProcessor(htmlWeb);
            return processor.DownloadAsync(new Request
            {
                DownloadPath = new Uri(uriString),
                SavePath = savePath
            }, cancellationToken);
        }

        /// <exception cref="T:System.OverflowException">
        ///     <paramref>s</paramref> represents a number less than
        ///     <see cref="F:System.Int32.MinValue"></see> or greater than <see cref="F:System.Int32.MaxValue"></see>.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException"><paramref>s</paramref> is null.</exception>
        /// <exception cref="T:System.FormatException"><paramref>s</paramref> is not in the correct format.</exception>
        private static int ParsePagesCount(HtmlDocument document)
        {
            var pagesTrNode = document.DocumentNode.ChildNodes[2].ChildNodes[3]
                .SelectSingleNode("div[4]//table[1]//tr[1]");
            var lastPageTdNode = pagesTrNode.ChildNodes[^2];
            var pages = int.Parse(lastPageTdNode.InnerText);
            return pages;
        }
    }
}