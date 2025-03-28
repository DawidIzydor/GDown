using Ghent.SimplyHentai;
using GHent.Data;
using GHent.Shared.CbrCreator;
using GHent.Shared.ProgressReporter;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
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
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppConfig.json", optional: false, reloadOnChange: true)
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            var dbPath = configuration["DbPath"];
            if (string.IsNullOrEmpty(dbPath))
            {
                throw new InvalidOperationException("DbPath is not defined in the AppConfig.json file.");
            }

            // TODO get rid of .Result
            services.AddSingleton((_) => DownloadManager.CreateAsync(dbPath).Result);

            services.AddSingleton<AlbumDownloadWindow>();

            services.AddSingleton<IImageSaver, HttpClientImageSaver>();
            services.AddSingleton<HtmlWeb>();

            services.AddSingleton<IEventableProgressReporter, EventableProgressReporter>();
            services.AddSingleton<DownloadWorker>();

            services.AddSingleton<ICbrCreator, ZipFileCbrCreator>();
            services.AddSingleton<CancellationTokenSource>();

            services.AddSingleton<NHentaiAlbumDownloader>();
            services.AddSingleton<NHentaiItemDownloader>();  
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetService<AlbumDownloadWindow>();
            mainWindow.Show();
        }
    }
}
