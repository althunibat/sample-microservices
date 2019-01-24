using System;
using Jaeger;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTracing.Util;

namespace Shared
{
    public static class JaegerServiceCollectionExtensions
    {
        public static IServiceCollection AddJaeger(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddSingleton(serviceProvider =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

                var config = Configuration.FromEnv(loggerFactory);

                var tracer = config.GetTracer();
                GlobalTracer.Register(tracer);
                return tracer;
            });
            return services;
        }
    }
}