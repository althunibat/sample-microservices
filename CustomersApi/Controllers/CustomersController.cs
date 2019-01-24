using System.Threading.Tasks;
using CustomersApi.DataStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CustomersApi.Controllers
{
    [Route("customers")]
    public class CustomersController : Controller
    {
        private readonly CustomerDbContext _dbContext;
        private readonly ILogger _logger;

        public CustomersController(CustomerDbContext dbContext, ILogger<CustomersController> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Json(await _dbContext.Customers.ToListAsync().ConfigureAwait(false));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Index(int id)
        {
            var customer = await _dbContext.Customers.FirstOrDefaultAsync(x => x.CustomerId == id)
                .ConfigureAwait(false);

            if (customer == null)
                return NotFound();

            // ILogger events are sent to OpenTracing as well!
            _logger.LogInformation("Returning data for customer {CustomerId}", id);

            return Json(customer);
        }
    }
}