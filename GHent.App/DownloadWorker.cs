using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Ghent.SimplyHentai;
using GHent.Shared.ProgressReporter;
using GHent.Shared.Request;
using GHent.Shared.CbrCreator;
using Microsoft.Extensions.DependencyInjection;
using GHent.Data;
using System.Linq;
using System.Collections.Concurrent;

namespace GHent.App
{
    public class DownloadWorker(IEventableProgressReporter progressReporter, 
        ICbrCreator cbrCreator,
        CancellationTokenSource cancellationTokenSource,
        IServiceProvider serviceProvider,
        DownloadManager gHentContext)
    {
        private readonly ConcurrentQueue<(string downloadPath, string savePath, bool saveCbr)> _downloadQueue = new();

        public bool IsRunning { get; private set; }

        private readonly SemaphoreSlim semaphore = new(1);

        private async Task UpdateStatusAsync(string downloadPath, string savePath, bool saveCbr, DownloadStatus status)
        {

            try
            {
                await semaphore.WaitAsync();

#pragma warning disable S6602
                var item = gHentContext.Items.FirstOrDefault(i=>i.Url == downloadPath);
#pragma warning restore S6602
                if (item is null)
                {
                    gHentContext.AddItem(new DownloadableItem
                    {
                        SaveCbr = saveCbr,
                        Url = downloadPath,
                        SavePath = savePath,
                        Status = status
                    });
                }
                else
                {
                    item.SaveCbr = saveCbr;
                    item.Url = downloadPath;
                    item.SavePath = savePath;
                    item.Status = status;

                }
                await gHentContext.SaveChanges();
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task EnqueueAsync(string downloadPath, string savePath, bool saveCbr)
        {
            _downloadQueue.Enqueue((downloadPath, savePath, saveCbr));
            await UpdateStatusAsync(downloadPath, savePath, saveCbr, DownloadStatus.Queued);
            await RunAsync();
        }

        private async Task RunAsync()
        {
            if (IsRunning) return;

            IsRunning = true;
            await Task.Run(ProcessQueueAsync);
        }

        private async Task ProcessQueueAsync()
        {
            while (_downloadQueue.TryDequeue(out var item))
            {
                var (downloadPath, savePath, saveCbr) = item;
                progressReporter.Report(ProgressType.Information, amount: 0, message: $"Processing next item in queue. Items left: {_downloadQueue.Count}");

                try
                {
                    var directoryPath = await DownloadElementAsync(savePath, new Uri(downloadPath));
                    CreateCbr(cbrCreator, savePath, saveCbr, directoryPath);

                    await UpdateStatusAsync(downloadPath, savePath, saveCbr, DownloadStatus.Finished);
                }
                catch (Exception)
                {
                    await UpdateStatusAsync(downloadPath, savePath, saveCbr, DownloadStatus.Error);
                }
            }

            IsRunning = false;
        }

        private static void CreateCbr(ICbrCreator cbrCreator, string savePath, bool saveCbr, string directoryPath)
        {
            if (saveCbr)
            {
                cbrCreator.CreateCbr(savePath, directoryPath);
            }
        }

        private async Task<string> DownloadElementAsync(string savePath, Uri downloadUri)
        {
            VerifyDirectoryExists(savePath);

            var directoryPath = await DownloadAsync(savePath, downloadUri,
                cancellationTokenSource.Token).ConfigureAwait(true);

            progressReporter.Report(
                ProgressType.Success, 
                amount: 0, 
                message: $"Finished {directoryPath}");


            return directoryPath;
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
#pragma warning disable S125
                // TODO: this needs to be reimplemented
                //var requestProcessor = new GHentaiAlbumRequestProcessor(new ActionableProgressReporter<string>((IProgressReporter<string> progress, string lastDone) => Application.Current.Dispatcher.Invoke(ProgressHandler, progress, lastDone)), htmlWeb);

                //return await requestProcessor.Download(albumRequest, cancellationToken)
                //    .ConfigureAwait(false);
#pragma warning restore S125
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

        public async Task EnqueueNotFinished()
        {
            await gHentContext.LoadItems();
            var items = gHentContext.Items.Where(i => i.Status != DownloadStatus.Finished).ToList();
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    await EnqueueAsync(item.Url, item.SavePath, item.SaveCbr);
                }
            }
            else
            {
                progressReporter.Report(ProgressType.Information, message: "No unfinished items to enqueue.", amount: 0);
            }
        }
    }
}