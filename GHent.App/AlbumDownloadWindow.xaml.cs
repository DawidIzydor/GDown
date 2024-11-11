using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using GHent.GHentai;
using GHent.Shared.ProgressReporter;

namespace GHent.App
{
    /// <summary>
    ///     Interaction logic for AlbumDownloadWindow.xaml
    /// </summary>
    public sealed partial class AlbumDownloadWindow : IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        private readonly object logBlock = new();
        private readonly DownloadWorker _downloadWorker;

        public AlbumDownloadWindow(DownloadWorker downloadWorker, IEventableProgressReporter eventableProgressReporter)
        {
            InitializeComponent();
            TaskbarItemInfo = new System.Windows.Shell.TaskbarItemInfo();
            SourceTextBox.Text = AppSettings.Default.LastDownloadPath;
            SavePath.Text = AppSettings.Default.LastSavePath;
            eventableProgressReporter.OnProgress += (sender, args) => {
                Application.Current.Dispatcher.Invoke(ProgressHandler, args);
            };
            eventableProgressReporter.OnReset += (sender, args) =>
            {

            };
            _downloadWorker = downloadWorker;
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
        private async void EnqueueButton_Click(object sender, RoutedEventArgs e)
        {
            await EnqueueAsync();
        }

        private async Task EnqueueAsync()
        {
            string downloadUrl = null;
            try
            {
                CancelButton.SetCurrentValue(VisibilityProperty, Visibility.Visible);

                var savePath = SavePath.Text;
                downloadUrl = SourceTextBox.Text;
                var saveCbr = saveCbrCheckbox.IsChecked ?? true;

                Log($"Adding to queue: URL: {downloadUrl}, Save Path: {savePath}, Save CBR: {saveCbr}");

                await _downloadWorker.EnqueueAsync(downloadUrl, savePath, saveCbr);
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
                Log($"Enqueued {downloadUrl}");
            }
        }

        private void ProgressHandler(object args)
        {

            if (args is ProgressEventArgs progressEventArgs)
            {
                double progressValue = progressEventArgs.Total != 0 ? progressEventArgs.Done * 100.0d / progressEventArgs.Total : 0;
                ProgressBar.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.ValueProperty, progressValue);

                TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressValueProperty, progressValue / 100.0d);
                switch (progressEventArgs.ProgressType)
                {
                    case ProgressType.Success:
                        TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressStateProperty, System.Windows.Shell.TaskbarItemProgressState.Normal);
                        break;

                    case ProgressType.Failure:
                        TaskbarItemInfo.SetCurrentValue(System.Windows.Shell.TaskbarItemInfo.ProgressStateProperty, System.Windows.Shell.TaskbarItemProgressState.Error);
                        break;

                    default:
                        break;
                }
                Log($"{progressEventArgs}");
            }
            else
            {
                Log($"{args}");
            }
        }


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

        private async void EnqueueNotFinished_Click(object sender, RoutedEventArgs e)
        {
            EnqueueNotFinished.SetCurrentValue(IsEnabledProperty, false);
            await _downloadWorker.EnqueueNotFinished();
        }

        private async void SourceTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.Enter)
            {
                await EnqueueAsync();
            }    
        }
    }
}