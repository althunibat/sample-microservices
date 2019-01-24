using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenTracing;
using Shared;

namespace OrdersApi.Controllers
{
    [Route("orders")]
    public class OrdersController : Controller
    {
        private readonly string _customerServiceUrl;
        private readonly HttpClient _httpClient;
        private readonly ITracer _tracer;

        public OrdersController(IHttpClientFactory factory, ITracer tracer, IConfiguration configuration)
        {
            _httpClient = factory.CreateClient("client");
            _tracer = tracer ?? throw new ArgumentNullException(nameof(tracer));
            _customerServiceUrl = configuration["CUSTOMER_SERVICE_URL"];
        }

        [HttpPost]
        public async Task<IActionResult> Index([FromBody] PlaceOrderCommand cmd)
        {
            if (cmd.CustomerId == null) return Ok();
            var customer = await GetCustomer(cmd.CustomerId.Value);

            _tracer.ActiveSpan?.Log(new Dictionary<string, object>
            {
                {"event", "OrderPlaced"},
                {"customer", cmd.CustomerId},
                {"customer_name", customer.Name},
                {"item_number", cmd.ItemNumber},
                {"quantity", cmd.Quantity}
            });

            return Ok();
        }

        private async Task<Customer> GetCustomer(int customerId)
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_customerServiceUrl + "customers/" + customerId)
            };

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<Customer>(body);
        }
    }
}