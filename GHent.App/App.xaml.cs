using Ghent.SimplyHentai;
using GHent.Shared.CbrCreator;
using GHent.Shared.ProgressReporter;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Windows;

namespace GHent.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<StartWindow>();
            services.AddSingleton<AlbumDownloadWindow>();

            services.AddSingleton<IImageSaver, HttpClientImageSaver>();
            services.AddSingleton<HtmlWeb>();

            services.AddSingleton<IEventableProgressReporter, EventableProgressReporter>();
            services.AddSingleton<DownloadWorker>();

            services.AddSingleton<ICbrCreator, CbrCreator>();
            services.AddSingleton<CancellationTokenSource>();

            services.AddSingleton<SimplyHentaiAlbumRequestProcessor>();
            services.AddSingleton<SimplyHentaiItemProcessor>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<StartWindow>();
            mainWindow.Show();
        }
    }
}
