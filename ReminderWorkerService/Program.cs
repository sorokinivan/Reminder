using Microsoft.EntityFrameworkCore;
using ReminderWorkerService.Data;
using Serilog.Events;
using Serilog;

namespace ReminderWorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                //.MinimumLevel.Information()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            builder.Services.AddSerilog();
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            //builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);
            builder.Services.AddScoped<ApplicationDbContext>(s => new ApplicationDbContext(optionsBuilder.Options));

            builder.Services.AddHostedService<Worker>();
            
            var host = builder.Build();
            host.Run();
        }
    }
}