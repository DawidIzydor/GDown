using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace GHent.App
{
    /// <summary>
    ///     Logika interakcji dla klasy StartWindow.xaml
    /// </summary>
    public partial class StartWindow
    {
        public StartWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private AlbumDownloadWindow _albumDownloadWindow;
        private CbrCreatorWindow _cbrWindow;
        private readonly IServiceProvider _serviceProvider;

        private void AlbumDownloadButton_Click(object sender, RoutedEventArgs e)
        {
            _albumDownloadWindow ??= _serviceProvider.GetService<AlbumDownloadWindow>();
            _albumDownloadWindow.Show();
        }

        private void GenerateCbrButton_Click(object sender, RoutedEventArgs e)
        {
            _cbrWindow ??= _serviceProvider.GetService<CbrCreatorWindow>();
            _cbrWindow.Show();
        }
    }
}