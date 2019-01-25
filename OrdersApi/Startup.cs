using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Shared;

namespace OrdersApi
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var checksBuilder = services.AddHealthChecks();
            var count = 0;
            foreach (var url in _configuration["ELK_URLS"].Split(";"))
                checksBuilder.AddElasticsearch(url, $"elk_{++count}");
            services.AddHttpClient("client")
                .AddPolicyHandler(GetRetryPolicy());
            // Registers and starts Jaeger (see Shared.JaegerServiceCollectionExtensions)
            services.AddJaeger(_configuration);

            // Enables OpenTracing instrumentation for ASP.NET Core, CoreFx, EF Core
            services.AddOpenTracing();

        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseMvcWithDefaultRoute();
            app.UseHealthChecks("/hc");
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
    }
}