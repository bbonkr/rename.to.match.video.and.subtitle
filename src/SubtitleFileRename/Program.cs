using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SubtitleFileRename.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SubtitleFileRename
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var services = new ServiceCollection();

            ConfigureServices(services);
            services.AddScoped<MainForm>();

            var provider = services.BuildServiceProvider();

            using (var form = provider.GetRequiredService<MainForm>())
            {
                Application.Run(form);
            }
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            // add dependency
            services
                .AddLogging(builder => builder.AddConsole())
                .AddScoped<RenameService>()
                ;
        }
    }
}
