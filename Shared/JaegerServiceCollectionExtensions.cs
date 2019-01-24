using System;
using Jaeger;
using Jaeger.Reporters;
using Jaeger.Samplers;
using Jaeger.Senders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

namespace Shared
{
    public static class JaegerServiceCollectionExtensions
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services, IConfiguration config)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var service = config["JAEGER_SERVICE_NAME"];
                int.TryParse(config["JAEGER_AGENT_PORT"] ?? "0", out var port);
                var tracer = new Tracer.Builder(service)
                    .WithLoggerFactory(loggerFactory)
                    .WithSampler(new ConstSampler(true))
                    .WithReporter(new CompositeReporter(new LoggingReporter(loggerFactory), new RemoteReporter.Builder()
                        .WithLoggerFactory(loggerFactory)
                        .WithSender(new UdpSender(config["JAEGER_AGENT_HOST"], port, 0))
                        .Build()))
                    .Build();
                GlobalTracer.Register(tracer);
                return tracer;
            });
            return services;
        }
    }
}