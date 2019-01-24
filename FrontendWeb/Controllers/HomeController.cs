using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared;

namespace FrontendWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _customerServiceUrl;
        private readonly HttpClient _httpClient;
        private readonly string _ordersServiceUrl;

        public HomeController(IHttpClientFactory factory, IConfiguration configuration)
        {
            _customerServiceUrl = configuration["CUSTOMER_SERVICE_URL"];
            _ordersServiceUrl = configuration["ORDERS_SERVICE_URL"];
            _httpClient = factory.CreateClient("client");
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PlaceOrder()
        {
            ViewBag.Customers = await GetCustomers();
            return View(new PlaceOrderCommand {ItemNumber = "ABC11", Quantity = 1});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(PlaceOrderCommand cmd)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Customers = await GetCustomers();
                return View(cmd);
            }

            var body = JsonConvert.SerializeObject(cmd);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_ordersServiceUrl + "orders"),
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            await _httpClient.SendAsync(request);

            return RedirectToAction("Index");
        }

        private async Task<IEnumerable<SelectListItem>> GetCustomers()
        {
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(_customerServiceUrl + "customers")
            };

            var response = await _httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<Customer>>(body)
                .Select(x => new SelectListItem {Value = x.CustomerId.ToString(), Text = x.Name});
        }
    }
}