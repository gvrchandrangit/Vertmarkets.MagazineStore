using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vertmarkets.Common.MSHttpClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections;
using Polly;
using Polly.Extensions.Http;
using System.Net.Http;

namespace Vertmarkets.MagazineStore
{
    public partial class Program
    {   
        static void Main(string[] args)
        {            
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            //var app = serviceProvider.GetService<SubscriptionProcessor>();
            //Task<string> tResponse = Task.Run(() => app.GetAnswersResult());
            //Console.WriteLine($"Answer Result: {tResponse.Result}");

            var fooApp = serviceProvider.GetService<Processor>();
            Console.WriteLine($"Processing Answer .....");
            Task<string> resp = Task.Run(() => fooApp.GetAnswersResult());            
            Console.WriteLine($"Answer Result: {resp.Result}");            
            Console.Read();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Add Logger
            services.AddLogging(configure =>
            {
                configure.AddConsole();
            })
                //.AddTransient<SubscriptionProcessor>()
                .AddTransient<Processor>()
                .AddSingleton<IHttpClientWrapper, HttpClientWrapper>();

            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            // Add access to generic IConfigurationRoot
            services.AddSingleton(configuration);

            services.AddHttpClient("HttpClient");
        }
    }
}
