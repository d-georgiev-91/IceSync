using Hangfire;
using IceSync.Data;
using IceSync.Web.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using IceSync.ApiClient;
using IceSync.ApiClient.Configs;
using IceSync.CommonAbstractions;

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

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddSingleton<IDateTimeService, DateTimeService>();
builder.Services.AddSingleton<IEnvironment, IceSync.CommonAbstractions.Environment>();
builder.Services.AddSingleton<IFileSystem, FileSystem>();

builder.Services.AddSingleton<IJwtSecurityTokenHandler, JwtSecurityTokenHandlerWrapper>();
builder.Services.AddSingleton<IJwtTokenManager, JwtTokenManager>();

builder.Services.Configure<ApiClientConfig>(builder.Configuration.GetSection(nameof(ApiClientConfig)));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IApiClient, ApiClient>();

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
        "SyncWorkflowsJob",
        service => service.SynchronizeWorkflows(),
        "*/30 * * * *",
        new RecurringJobOptions { TimeZone = TimeZoneInfo.Local });
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Workflow}/{action=Index}/{id?}");

app.Run();
