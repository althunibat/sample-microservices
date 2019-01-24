using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TrafficGenerator
{
    public class Worker : BackgroundService
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger, HttpClient client, IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                _client.BaseAddress = new Uri(_configuration["CUSTOMER_SERVICE_URL"]);

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Requesting customers");

                    var response = await _client.GetAsync("customers", stoppingToken);

                    _logger.LogInformation($"Response was '{response.StatusCode}'");

                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {
                /* Application should be stopped -> no-op */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
        }
    }
}