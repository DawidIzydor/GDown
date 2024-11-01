using System;
using System.Threading;
using System.Windows;
using GHent.GHentai;
using Ghent.SimplyHentai;
using HtmlAgilityPack;
using GHent.Shared.ProgressReporter;
using GHent.Shared.CbrCreator;

namespace GHent.App
{
    /// <summary>
    ///     Interaction logic for AlbumDownloadWindow.xaml
    /// </summary>
    public sealed partial class AlbumDownloadWindow : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly HtmlWeb _htmlWeb = new();
        private readonly IImageSaver _imageSaver;
        private readonly IProgressReporter<ProgressData<string>> _progressReporter;
        private readonly DownloadWorker _downloadWorker;
        private readonly ICbrCreator _cbrCreator;

        public AlbumDownloadWindow()
        {
            InitializeComponent();
            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            SourceTextBox.Text = AppSettings.Default.LastDownloadPath;
            SavePath.Text = AppSettings.Default.LastSavePath;

            _progressReporter =
                    new ActionableProgressReporter<ProgressData<string>>((
                        IProgressReporter<ProgressData<string>> progress, ProgressData<string> lastDone) 
                        => Application.Current.Dispatcher.Invoke(ProgressHandler, progress, lastDone));
            _imageSaver = new HttpClientImageSaver(_progressReporter);
            _cbrCreator = new CbrCreator(_progressReporter);
            _downloadWorker = new DownloadWorker(_progressReporter, _htmlWeb, _imageSaver, _cbrCreator,_cancellationTokenSource);
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
            string downloadUrl = null;
            try
            {
                ResetDownloadUiElements();

                var savePath = SavePath.Text;
                downloadUrl = SourceTextBox.Text;
                var saveCbr = saveCbrCheckbox.IsChecked ?? true;

                Log($"Adding to queue: URL: {downloadUrl}, Save Path: {savePath}, Save CBR: {saveCbr}");

                _downloadWorker.Enqueue(downloadUrl, savePath, saveCbr);
                _downloadWorker.Run();
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Download cancelled.");
                Log("Cancelled");
            }
            catch (TransferExceededException ex)
            {
                MessageBox.Show(ex.Message);
                await _cancellationTokenSource.CancelAsync();
            }
            catch (UriFormatException)
            {
                MessageBox.Show("Wrong url format!");
            }
            catch (Exception ex)
            {
                Log($"Exception during downloading: {ex.Message}: {ex.StackTrace}");
            }
            finally
            {
                DownloadButton.SetCurrentValue(VisibilityProperty, Visibility.Visible);
                CancelButton.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
                Log($"Enqueued {downloadUrl}");
            }
        }



        private void ResetDownloadUiElements()
        {
            DownloadButton.SetCurrentValue(VisibilityProperty, Visibility.Collapsed);
            CancelButton.SetCurrentValue(VisibilityProperty, Visibility.Visible);
        }

        private void ProgressHandler(IProgressReporter<ProgressData<string>> reporter, ProgressData<string> lastDone)
        {
            double progressValue = reporter.Total != 0 ? reporter.Done * 100.0d / reporter.Total : 0;
            ProgressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.ValueProperty, progressValue);

            TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressValueProperty, progressValue/100.0d);
            switch (lastDone.Type)
            {
                case ProgressType.Success:
                    Log($"Downloaded {lastDone.Value}");
                    TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressStateProperty, System.Windows.Shell.TaskbarItemProgressState.Normal);
                    break;

                case ProgressType.Failure:
                    Log($"Problem downloading {lastDone.Value}: {lastDone.Information}");
                    TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressStateProperty, System.Windows.Shell.TaskbarItemProgressState.Error);
                    break;

                case ProgressType.Skipped:
                    break;

                case ProgressType.Information:
                    Log($"{lastDone.Value}: {lastDone.Information}");
                    break;

                default:
                    Log($"Unknown progress type: {lastDone.Value}, {lastDone.Type}, {lastDone.Information}");
                    break;
            }
        }

        private readonly object logBlock = new();
        private void Log(string str)
        {
            lock (logBlock)
            {
                LogBlock.SetCurrentValue(System.Windows.Controls.TextBlock.TextProperty, $"{DateTime.Now:g} {str}{Environment.NewLine}{LogBlock.Text}");
            }
        }

        /// <exception cref="T:System.AggregateException">
        ///     An aggregate exception containing all the exceptions thrown by the
        ///     registered callbacks on the associated <see cref="T:System.Threading.CancellationToken" />.
        /// </exception>
        private void CancelButton_Click(object sender, RoutedEventArgs e) => _cancellationTokenSource.Cancel();

        public void Dispose() => _cancellationTokenSource?.Dispose();
    }
}