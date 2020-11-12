using System.Collections.Generic;
using System.Linq;
using System.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace SurpriseText
{
    class Program
    {
        static void Main(string[] args)
        {

            var services = new ServiceCollection();

            ConfigureServices(services, args);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                var menu = serviceProvider.GetService<Menu>();
                    menu.Run();
            }

        }

        private static void ConfigureServices(ServiceCollection services, string []files)
        {

            services.AddTransient<Menu>(
                menu => new Menu(files.ToList(), menu.GetService<ILogger<Menu>>()));

            var serilogLogger = new LoggerConfiguration()
                .WriteTo.File("menu.log")
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(serilogLogger, true);
            });
        }
    }
}
