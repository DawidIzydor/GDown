using System;
using System.Windows;

namespace GHent.App
{
    /// <summary>
    ///     Logika interakcji dla klasy StartWindow.xaml
    /// </summary>
    public partial class StartWindow
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private AlbumDownloadWindow _albumDownloadWindow;
        private CbrCreatorWindow _cbrWindow;

        private void AlbumDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_albumDownloadWindow == null)
            {
                _albumDownloadWindow = new AlbumDownloadWindow();
                _albumDownloadWindow.Show();
            }
            else
            {
                try
                {
                    _albumDownloadWindow.Show();
                }
                catch (InvalidOperationException)
                {
                    _albumDownloadWindow.Close();
                    _albumDownloadWindow = new AlbumDownloadWindow();
                    _albumDownloadWindow.Show();
                }
                _albumDownloadWindow.Focus();
            }
        }

        private void GenerateCbrButton_Click(object sender, RoutedEventArgs e)
        {
            if (_cbrWindow == null)
            {
                _cbrWindow = new CbrCreatorWindow();
                _cbrWindow.Show();
            }
            else
            {
                try
                {
                    _cbrWindow.Show();
                }
                catch (InvalidOperationException)
                {
                    _cbrWindow.Close();
                    _cbrWindow = new CbrCreatorWindow();
                    _cbrWindow.Show();
                }
                _cbrWindow.Focus();
            }
        }
    }
}