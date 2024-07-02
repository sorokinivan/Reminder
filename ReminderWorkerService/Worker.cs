using Microsoft.EntityFrameworkCore;
using ReminderWorkerService.Data;

namespace ReminderWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _provider;

        public Worker(ILogger<Worker> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                using (IServiceScope scope = _provider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;
                    
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var t = await context.ToDoThings.FirstOrDefaultAsync();
                    Console.WriteLine(t.Title);
                }
                    

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
