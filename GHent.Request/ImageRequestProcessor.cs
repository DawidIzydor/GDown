using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GHent.Models;
using GHent.RequestProcessor.Singleton;

namespace GHent.RequestProcessor
{
    public static class ImageRequestProcessor
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(5);
        /// <exception cref="T:System.InvalidOperationException">The local file specified by fileName is in use by another thread.</exception>
        /// <exception cref="T:System.Net.WebException">
        ///     The URI formed by combining
        ///     <see cref="P:System.Net.WebClient.BaseAddress"></see> and address is invalid.   -or-   An error occurred while
        ///     downloading the resource.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        ///     The >address parameter is null.   -or-   The fileName parameter is
        ///     null.
        /// </exception>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:GHent.RequestProcessor.TransferExceededException">Transfer was exceeded</exception>
        /// <exception cref="T:System.ObjectDisposedException">
        ///     The associated
        ///     <see cref="T:System.Threading.CancellationTokenSource"></see> has been disposed.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref>path1</paramref> or <paramref>path2</paramref> contains one or
        ///     more of the invalid characters defined in <see cref="M:System.IO.Path.GetInvalidPathChars"></see>.
        /// </exception>
        public static async Task<string> DownloadAsync(ImageRequest imageRequest, CancellationToken cancellationToken)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);
                var extractDirectImgLink = await ExtractDirectImgLinkAsync(imageRequest.DownloadPath, cancellationToken)
                    .ConfigureAwait(false);
                var imageName = extractDirectImgLink.Split('/').Last();

                if (WasTransferExceeded(imageName))
                {
                    throw new TransferExceededException("Transfer was exceeded");
                }

                string fileName = GetFileName(imageRequest, imageName);

                if (File.Exists(fileName))
                {
                    return fileName;
                }

                cancellationToken.ThrowIfCancellationRequested();
                await SaveFileAsync(extractDirectImgLink, fileName, cancellationToken)
                    .ConfigureAwait(false);

                return fileName;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private static string GetFileName(ImageRequest imageRequest, string imageName)
        {
            return Path.Combine(imageRequest.SavePath, imageName);
        }

        private static async Task SaveFileAsync(string extractDirectImgLink, string fileName, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            using var response = await client.GetAsync(extractDirectImgLink, cancellationToken);
            using var content = response.Content;
            await using var stream = await content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await stream.CopyToAsync(fileStream, cancellationToken);
        }

        private static bool WasTransferExceeded(string imageName)
        {
            return imageName == "509.gif";
        }

        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        private static async Task<string> ExtractDirectImgLinkAsync(Uri netPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var extractedNl = await ExtractNlUrlAsync(netPath, cancellationToken).ConfigureAwait(false);
            var a = (await HtmlWebSingleton.Instance
                .LoadFromWebAsync(netPath + "?nl=" + extractedNl, cancellationToken)
                .ConfigureAwait(false)).GetElementbyId("i3");
            return a.SelectSingleNode(a.XPath + "//a//img").Attributes["src"].Value;
        }

        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        private static async Task<string> ExtractNlUrlAsync(Uri netPath, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var nl = (await HtmlWebSingleton.Instance.LoadFromWebAsync(netPath.ToString(), cancellationToken)
                    .ConfigureAwait(false))
                // ReSharper disable once StringLiteralTypo
                .GetElementbyId("loadfail");
            var nlUrl = nl.Attributes["onclick"].Value;
            nlUrl = nlUrl.Remove(0, nlUrl.IndexOf('\'') + 1);
            nlUrl = nlUrl.Remove(nlUrl.IndexOf('\''));
            return nlUrl;
        }
    }
}