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
                var log = loggerFactory.CreateLogger<Constants>();
                var service = config["JAEGER_SERVICE_NAME"];
                int.TryParse(config["JAEGER_AGENT_PORT"] ?? "6831", out var port);
                var host = config["JAEGER_AGENT_HOST"];
                log.LogInformation($"Service: {service}");
                log.LogInformation($"Agent: {host}:{port}");

                var sampler = new Configuration.SamplerConfiguration(loggerFactory)
                    .WithType(ConstSampler.Type)
                    .WithParam(1);
                var sender = new Configuration.SenderConfiguration(loggerFactory)
                    .WithAgentHost(host)
                    .WithAgentPort(port);
                var reporter =  new Configuration.ReporterConfiguration(loggerFactory)
                    .WithLogSpans(true)
                    .WithSender(sender);

                var tracer = new Configuration(service, loggerFactory)
                    .WithSampler(sampler)
                    .WithReporter(reporter)
                    .GetTracer();
                GlobalTracer.Register(tracer);
                return tracer;
            });
            return services;
        }
    }
}