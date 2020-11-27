using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SurpriseText
{
    class Program
    {
        private static void Main(string[] args)
        {

            var services = new ServiceCollection();

            ConfigureServices(services, args);

            using (var serviceProvider = services.BuildServiceProvider())
            {
                var menu = serviceProvider.GetService<Menu>();
                    menu.Run();
            }

        }

        private static void ConfigureServices(ServiceCollection services, string []files)
        {
            // TODO: find a way to instantiate logger class for all classes where logging is needed 
            services.AddTransient(
                menu => new Menu(files.ToList(), menu.GetService<ILogger<Menu>>()));

            var serilog = new LoggerConfiguration()
                .WriteTo.File("menu.log")
                .CreateLogger();

            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddSerilog(serilog, true);
            });
        }
    }
}
