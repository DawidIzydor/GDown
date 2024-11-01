using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ghent.SimplyHentai;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using System.Collections.Generic;
using GHent.Shared.CbrCreator;
using Microsoft.Extensions.DependencyInjection;

namespace GHent.App
{
    public class DownloadWorker(IEventableProgressReporter progressReporter, 
        ICbrCreator cbrCreator,
        CancellationTokenSource cancellationTokenSource,
        IServiceProvider serviceProvider)
    {
        private readonly Queue<(string downloadPath, string savePath, bool saveCbr)> _downloadQueue = new();
        private Thread _backgroundThread = null;

        public bool IsRunning { get; private set; }
        private readonly object runLock = new();

        public void Enqueue(string downloadPath, string savePath, bool saveCbr)
        {
            lock (_downloadQueue)
            {
                _downloadQueue.Enqueue((downloadPath, savePath, saveCbr));
            }
        }

        private async Task DoWork()
        {
            lock (runLock) { 
                IsRunning = true;
            }
            try
            {
                while (_downloadQueue.Count > 0)
                {
                    (var downloadUrl, var savePath, var saveCbr) = _downloadQueue.Dequeue();

                    progressReporter.Report(
                        ProgressType.Information, 
                        amount: 0, 
                        message: $"Processing next item in queue. Left in queue: {_downloadQueue.Count}");

                    var downloadUri = new Uri(downloadUrl);

                    await DownloadElementAsync(savePath, downloadUri, saveCbr);
                }
            }
            finally
            {
                lock (runLock)
                {
                    IsRunning = false;
                }
            }
        }

        public void Run()
        {
            if(_backgroundThread is null)
            {
                _backgroundThread = new Thread(async () => await DoWork())
                {
                    IsBackground = true
                };
                _backgroundThread.Start();
            }
            else
            {
                if(!IsRunning)
                {
                    _backgroundThread = new Thread(async () => await DoWork());
                    _backgroundThread.Start();
                }
            }

        }

        private async Task DownloadElementAsync(string savePath, Uri downloadUri, bool saveCbr)
        {
            VerifyDirectoryExists(savePath);

            var directoryPath = await DownloadAsync(savePath, downloadUri,
                cancellationTokenSource.Token).ConfigureAwait(true);

            progressReporter.Report(
                ProgressType.Success, 
                amount: 0, 
                message: $"Finished {directoryPath}");

            if (saveCbr)
            {
                cbrCreator.CreateCbr(savePath, directoryPath);
            }
        }

        /// <exception cref="T:System.IO.IOException">
        ///     The directory specified by path is a file.   -or-   The network name is not
        ///     known.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid (for example, it is on an
        ///     unmapped drive).
        /// </exception>
        /// <exception cref="T:System.OperationCanceledException">The token has had cancellation requested.</exception>
        /// <exception cref="T:System.OverflowException">
        ///     <paramref>s</paramref> represents a number less than
        ///     <see cref="F:System.Int32.MinValue"></see> or greater than <see cref="F:System.Int32.MaxValue"></see>.
        /// </exception>
        /// <exception cref="T:GHent.RequestProcessor.TransferExceededException">Transfer was exceeded</exception>
        /// <exception cref="T:System.AggregateException"></exception>
        private async Task<string> DownloadAsync(string savePath, Uri downloadUri,
            CancellationToken cancellationToken)
        {
            SaveLastUsedPaths(downloadUri, savePath);

            var albumRequest = new Request
            {
                DownloadPath = downloadUri,
                SavePath = savePath
            };

            if (downloadUri.Host == "simplyhentai.org" || downloadUri.Host == "nhentai.net")
            {

                var requestProcessor = serviceProvider.GetRequiredService<SimplyHentaiAlbumRequestProcessor>();

                return await requestProcessor.Download(albumRequest, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new NotImplementedException("Need to reimplement GHent");
                //var requestProcessor = new GHentaiAlbumRequestProcessor(new ActionableProgressReporter<string>((IProgressReporter<string> progress, string lastDone) => Application.Current.Dispatcher.Invoke(ProgressHandler, progress, lastDone)), htmlWeb);

                //return await requestProcessor.Download(albumRequest, cancellationToken)
                //    .ConfigureAwait(false);
            }
        }


        private static void SaveLastUsedPaths(Uri downloadUri, string savePath)
        {
            AppSettings.Default.LastDownloadPath = downloadUri.ToString();
            AppSettings.Default.LastSavePath = savePath;
            AppSettings.Default.Save();
        }

        /// <exception cref="T:System.IO.IOException">
        ///     The directory specified by <paramref /> is a file.
        ///     -or-
        ///     The network name is not known.
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid (for example, it is on an
        ///     unmapped drive).
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">MessageBox result not found</exception>
        private static void VerifyDirectoryExists(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                return;
            }

            var result = MessageBox.Show("Save directory does not exist, create it?", "Question",
                MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Directory.CreateDirectory(savePath);
                    return;
                case MessageBoxResult.No:
                    throw new DirectoryNotFoundException();
                default:
                    throw new InvalidOperationException($"Result not found: {result}");
            }
        }

    }
}