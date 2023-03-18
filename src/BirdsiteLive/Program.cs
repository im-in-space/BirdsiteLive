using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Services;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BirdsiteLive
{
    public class Program
    {
        public static string VERSION = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString(3) + "+im-in-space";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddHostedService<FederationService>();
                    services.AddHostedService<TweetCleanUpService>();
                });
    }
}
