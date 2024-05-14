using Hangfire;
using IceSync.Data;
using IceSync.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net.Http.Headers;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure HttpClient and JWT authentication
builder.Services.AddHttpClient("ApiHttpClient")
    .ConfigureHttpClient((sp, client) =>
    {
        var tokenService = sp.GetRequiredService<IceSync.Web.Services.ITokenService>();
        var token = Task.Run(() => tokenService.GetTokenAsync()).GetAwaiter().GetResult();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    });

builder.Services.AddSingleton<IceSync.Web.Services.ITokenService, IceSync.Web.Services.TokenService>();

// Configure Hangfire to use SQL Server
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();

// Register the workflow synchronization job
builder.Services.AddScoped<WorkflowSyncService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseHangfireDashboard(); // Hangfire Dashboard

// Register the job after the app has started
var hostApplicationLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
hostApplicationLifetime.ApplicationStarted.Register(() =>
{
    RecurringJob.AddOrUpdate<WorkflowSyncService>(
        "SyncWorkflowsJob", // recurringJobId
        service => service.SynchronizeWorkflows(),
        "*/30 * * * *", // CRON expression for every 30 minutes
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Workflow}/{action=Index}/{id?}");

app.Run();
