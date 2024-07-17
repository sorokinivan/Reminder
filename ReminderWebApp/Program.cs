using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ReminderWebApp.Data;
using ReminderWebApp.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using ReminderWebApp.Services.ToDoThingService;
using ReminderWebApp.Services.UserService;
using Serilog;
using Serilog.Events;
using ReminderWebApp.Helpers.AuthorizationHelpers;
using Microsoft.AspNetCore.Authorization;

namespace ReminderWebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                //.MinimumLevel.Information()
                //.MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                //.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                //.WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();
            builder.Services.AddSerilog();
            builder.Services.AddScoped<IAuthorizationHandler, IsToDoThingOwnerHandler>();
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("IsToDoThingOwner", policyBuilder =>
                {
                    policyBuilder.AddRequirements(new IsToDoThingOwnerRequirement());
                });
            });
            // Add services to the container.
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
            builder.Services.AddHealthChecks();
            builder.Services.AddTransient<IEmailSender, EmailSender>();

            builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();
            builder.Services.AddRazorPages();

            builder.Services.AddTransient<IToDoThingService, ToDoThingService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.MapHealthChecks("/healthz");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
