using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ReminderWebApp.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReminderWebApp.IntegrationTests
{
    public class CustomApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            builder.UseEnvironment("Development");

            builder.ConfigureTestServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }
                Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Information()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
                services.AddSerilog();
                services.AddHealthChecks();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                });
            });
        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    _connection.Close();
        //}
    }
}
