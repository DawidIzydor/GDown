using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GHent.Models;
using GHent.RequestProcessor;

namespace GHent.App
{
    /// <summary>
    ///     Interaction logic for AlbumDownloadWindow.xaml
    /// </summary>
    public partial class AlbumDownloadWindow
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public AlbumDownloadWindow()
        {
            InitializeComponent();
            SourceTextBox.Text = AppSettings.Default.LastDownloadPath;
            SavePath.Text = AppSettings.Default.LastSavePath;
        }

        /// <exception cref="T:System.AggregateException">
        ///     An aggregate exception containing all the exceptions thrown by the
        ///     registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.
        /// </exception>
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Started");
                var savePathText = SavePath.Text;
                ProgressBar.Value = 0;
                DownloadButton.Visibility = Visibility.Collapsed;
                CancelButton.Visibility = Visibility.Visible;
                var directory = await DownloadAsync(new Progress<DownloadProgressReport>(ProgressHandler),
                    _cancellationTokenSource.Token).ConfigureAwait(true);

                var filename = directory.Split('/').Last();
                ZipFile.CreateFromDirectory(directory, Path.Combine(savePathText, filename));
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
                Log($"Exception during downloading: {ex.Message}");
            }
            finally
            {
                DownloadButton.Visibility = Visibility.Visible;
                CancelButton.Visibility = Visibility.Collapsed;
                Log("Finished");
            }
        }

        private void ProgressHandler(DownloadProgressReport report)
        {
            ProgressBar.Value = report.All * 100.0f / report.Finished;
            Log($"Downloaded {report.FinishedPath}");
        }

        private void Log(string str)
        {
            lock (LogBlock)
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
        /// <exception cref="T:System.ArgumentNullException"><paramref /> is <see langword="null" />.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">MessageBox result not found</exception>
        private Task<string> DownloadAsync(IProgress<DownloadProgressReport> progress,
            CancellationToken cancellationToken)
        {
            var savePath = SavePath.Text;

            var downloadUri = new Uri(SourceTextBox.Text);


            SaveLastUsedPaths(downloadUri, savePath);
            VerifyDirectory(savePath);

            var albumRequest = new AlbumRequest
            {
                DownloadPath = downloadUri,
                SavePath = savePath
            };

            return AlbumRequestProcessor.DownloadAsync(albumRequest, progress, cancellationToken);
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
        private static void VerifyDirectory(string savePath)
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
        }

        /// <exception cref="T:System.AggregateException">
        ///     An aggregate exception containing all the exceptions thrown by the
        ///     registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.
        /// </exception>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }
    }
}