using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using SubtitleFileRename.Services;
using SubtitleFileRename.v2.App.ViewModels;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SubtitleFileRename.v2.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            provider = services.BuildServiceProvider();
        }


        private void ConfigureServices(IServiceCollection services)
        {
            // add dependency
            services
                .AddLogging(builder => builder.AddConsole())
                .AddScoped<RenameService>()
                ;

            // add ViewModels
            services.AddSingleton<MainWindowViewModel>();

            services.AddSingleton<MainWindow>();
        }

        /// <summary>
        /// Startup event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = provider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private readonly IServiceProvider provider;       
    }
}
