using System;
using System.IO;
using System.Linq;
using Elasticsearch.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Shared;

namespace OrdersApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return new WebHostBuilder()
                .UseKestrel(options => options.AddServerHeader = false)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    var jsonCfg = config.Build();

                    InitializeLogs(jsonCfg);
                })
                .UseStartup<Startup>()
                .UseSerilog();
        }

        private static void InitializeLogs(IConfiguration config)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Default", LogEventLevel.Warning)
                .MinimumLevel.Override("Jaeger.Reporters", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new StaticConnectionPool(config["ELK_URLS"].Split(";").Select(url => new Uri(url)), true,
                        DateTimeProvider.Default)));
            Log.Logger = logConfig.CreateLogger();
        }
    }
}