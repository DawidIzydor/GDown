using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GHent.GHentai;
using GHent.Models;
using Ghent.SimplyHentai;
using HtmlAgilityPack;

namespace GHent.App
{
    /// <summary>
    ///     Interaction logic for AlbumDownloadWindow.xaml
    /// </summary>
    public sealed partial class AlbumDownloadWindow : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private HtmlWeb _htmlWeb = new HtmlWeb();

        public AlbumDownloadWindow()
        {
            InitializeComponent();
            SourceTextBox.Text = AppSettings.Default.LastDownloadPath;
            SavePath.Text = AppSettings.Default.LastSavePath;
        }

        /// <exception cref="T:System.OverflowException">
        ///     <paramref>s</paramref> represents a number less than
        ///     <see cref="F:System.Int32.MinValue"></see> or greater than <see cref="F:System.Int32.MaxValue"></see>.
        /// </exception>
        /// <exception cref="T:System.IO.DirectoryNotFoundException">
        ///     The specified path is invalid (for example, it is on an
        ///     unmapped drive).
        /// </exception>
        /// <exception cref="T:System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="T:System.IO.IOException">
        ///     The directory specified by path is a file.   -or-   The network name is not
        ///     known.
        /// </exception>
        /// <exception cref="T:System.AggregateException">
        ///     An aggregate exception containing all the exceptions thrown by the
        ///     registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.
        /// </exception>
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Started");
                ResetDownloadUiElements();

                var savePath = SavePath.Text;
                var downloadUri = new Uri(SourceTextBox.Text);

                VerifyDirectoryExists(savePath);

                var directoryPath = await DownloadAsync(new Progress<DownloadProgressReport>(ProgressHandler), savePath, downloadUri,
                    _cancellationTokenSource.Token).ConfigureAwait(true);

                var saveCbr = saveCbrCheckbox.IsChecked ?? false;
                if(saveCbr)
                {
                    string cbrFileName = GetCbrFileName(savePath, directoryPath);
                    Log($"Will save {directoryPath} into {cbrFileName}");
                    ZipFile.CreateFromDirectory(directoryPath, cbrFileName);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Download cancelled.");
                Log("Cancelled");
            }
            catch (TransferExceededException ex)
            {
                MessageBox.Show(ex.Message);
                _cancellationTokenSource.Cancel();
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Wrong url format!");
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (Exception ex)
            {
                Log($"Exception during downloading: {ex.Message}: {ex.StackTrace}");
            }
            finally
            {
                DownloadButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;
                Log("Finished");
            }
        }

        private static string GetCbrFileName(string savePathText, string directoryPath)
        {
            var dirSplit = directoryPath.Split('\\');
            var filename = dirSplit[dirSplit.Length - 1] != "" ? dirSplit[dirSplit.Length - 1] : dirSplit[dirSplit.Length - 2];
            var cbrFileName = Path.Combine(savePathText, filename) + ".cbr";
            return cbrFileName;
        }

        private void ResetDownloadUiElements()
        {
            ProgressBar.Value = 0;
            DownloadButton.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
        }

        private void ProgressHandler(DownloadProgressReport report)
        {
            ProgressBar.Value = report.All * 100.0f / report.Finished;
            Log($"Downloaded {report.FinishedPath}");
        }

        private readonly object logBlock = new object();
        private void Log(string str)
        {
            lock (logBlock)
            {
                LogBlock.Text = $"{DateTime.Now:g} {str}{Environment.NewLine}{LogBlock.Text}";
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
        private async Task<string> DownloadAsync(IProgress<DownloadProgressReport> progress, string savePath, Uri downloadUri,
            CancellationToken cancellationToken)
        {
            SaveLastUsedPaths(downloadUri, savePath);

            var albumRequest = new Request
            {
                DownloadPath = downloadUri,
                SavePath = savePath
            };

            if(downloadUri.Host == "simplyhentai.org")
            {
                var requestProcessor = new SimplyHentaiAlbumRequestProcessor(progress, _htmlWeb);

                return await requestProcessor.Download(albumRequest, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var requestProcessor = new GHentaiAlbumRequestProcessor(progress, _htmlWeb);

                return await requestProcessor.Download(albumRequest, cancellationToken)
                    .ConfigureAwait(false);
            }
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
        private static bool VerifyDirectoryExists(string savePath)
        {
            if (Directory.Exists(savePath))
            {
                return true;
            }

            var result = MessageBox.Show("Save directory does not exist, create it?", "Question",
                MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Directory.CreateDirectory(savePath);
                    return true;
                case MessageBoxResult.No:
                    throw new DirectoryNotFoundException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), "MessageBox result not found");
            }
        }

        private static void SaveLastUsedPaths(Uri downloadUri, string savePath)
        {
            AppSettings.Default.LastDownloadPath = downloadUri.ToString();
            AppSettings.Default.LastSavePath = savePath;
            AppSettings.Default.Save();
        }

        private void BrowseSavePathButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotSupportedException("Not implemented yet.");
        }

        /// <exception cref="T:System.AggregateException">
        ///     An aggregate exception containing all the exceptions thrown by the
        ///     registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.
        /// </exception>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        public void Dispose()
        {
            _cancellationTokenSource?.Dispose();
        }
    }
}