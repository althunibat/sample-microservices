using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using Shared;

namespace TrafficGenerator
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    var jsonCfg = config.Build();

                    InitializeLogs(jsonCfg);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Registers and starts Jaeger (see Shared.JaegerServiceCollectionExtensions)
                    services.AddJaeger(hostContext.Configuration);
                    services.AddHttpClient("client")
                        .AddPolicyHandler(GetRetryPolicy());
                    services.AddOpenTracing();
                    services.AddHostedService<Worker>();
                })
                .UseSerilog();
            await builder.RunConsoleAsync();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            var random = new Random();
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                                      + TimeSpan.FromMilliseconds(random.Next(0, 100)));
        }

        private static void InitializeLogs(IConfiguration config)
        {
            var logConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Default", LogEventLevel.Warning)
                .MinimumLevel.Override("Jaeger.Reporters", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                .Enrich.FromLogContext();
            logConfig = logConfig
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                    new StaticConnectionPool(config["ELK_URLS"].Split(";").Select(url => new Uri(url)), true,
                        DateTimeProvider.Default)));
            Log.Logger = logConfig.CreateLogger();
        }
    }
}