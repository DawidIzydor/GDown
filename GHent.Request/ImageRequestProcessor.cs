using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GHent.Models;
using GHent.RequestProcessor.Singleton;

namespace GHent.RequestProcessor
{
    public static class ImageRequestProcessor
    {
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
            using var wc = new WebClient();
            var extractDirectImgLink = await ExtractDirectImgLinkAsync(imageRequest.DownloadPath, cancellationToken)
                .ConfigureAwait(false);
            var imageName = extractDirectImgLink.Split('/').Last();
            if (imageName == "509.gif")
            {
                throw new TransferExceededException("Transfer was exceeded");
            }

            var fileName = Path.Combine(imageRequest.SavePath, imageName);

            if (File.Exists(fileName))
            {
                return fileName;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await wc.DownloadFileTaskAsync(extractDirectImgLink, fileName)
                .ConfigureAwait(false);
            return fileName;
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