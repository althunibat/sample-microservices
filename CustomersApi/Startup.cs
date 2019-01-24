using System;
using CustomersApi.DataStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CustomersApi
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
            // Adds an InMemory-Sqlite DB to show EFCore traces.
            services
                .AddEntityFrameworkSqlite()
                .AddDbContextPool<CustomerDbContext>(options =>
                {
                    var connectionStringBuilder = new SqliteConnectionStringBuilder
                    {
                        DataSource = ":memory:",
                        Mode = SqliteOpenMode.Memory,
                        Cache = SqliteCacheMode.Shared
                    };
                    var connection = new SqliteConnection(connectionStringBuilder.ConnectionString);

                    // Hack: EFCore resets the DB for every connection so we keep the connection open.
                    // This is obviously just demo code :)
                    connection.Open();
                    connection.EnableExtensions(true);

                    options.UseSqlite(connection);
                });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var checksBuilder = services.AddHealthChecks();
            var count = 0;
            foreach (var url in _configuration["ELK_URLS"].Split(";"))
                checksBuilder.AddElasticsearch(url, $"elk_{++count}");
        }

        public void Configure(IApplicationBuilder app)
        {
            // Load some dummy data into the InMemory db.
            BootstrapDataStore(app.ApplicationServices);

            app.UseDeveloperExceptionPage();
            app.UseHealthChecks("/hc");
            app.UseMvc();
        }

        public void BootstrapDataStore(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<CustomerDbContext>();
                dbContext.Seed();
            }
        }
    }
}