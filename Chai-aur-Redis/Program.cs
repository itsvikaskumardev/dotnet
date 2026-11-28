using Chai_aur_Redis.Data;
using Chai_aur_Redis.Email_Queue;
using Chai_aur_Redis.OTP_FLow_TTL;
using Chai_aur_Redis.Order_Confirmation_Jobs;
using Chai_aur_Redis.SetupLocal.Endpoints;
using Chai_aur_Redis.SetupLocal.Services;
using Chai_aur_Redis.SetupLocal.Services.Interfaces;
using Chai_aur_Redis.SiteBanner.Endpoints;
using Chai_aur_Redis.User_Json_Hash;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure OpenAPI
builder.Services.AddOpenApi();

// Register EF Core DbContext with PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis connection multiplexer
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
    return ConnectionMultiplexer.Connect(connectionString);
});

// Register application services
builder.Services.AddScoped<IDatabaseHealthService, DatabaseHealthService>();

// Register Order Confirmation & Background Worker Jobs
builder.Services.AddSingleton<EmailQueue>();
builder.Services.AddHostedService<EmailWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Map standard OpenAPI JSON document (/openapi/v1.json)
    app.MapOpenApi();

    // Map modern Scalar API Reference UI (/scalar/v1)
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Chai aur Redis - API Documentation")
               .WithTheme(ScalarTheme.Moon);
    });
}

app.UseHttpsRedirection();

// Map Minimal API Endpoints (Organized into separate Scalar modules)
app.MapDatabaseEndpoints();
app.MapSiteBannerEndpoints();
app.MapOTPFlowEndpoints();
app.MapUserEndpoints();
app.MapEmailQueueEndpoints();
app.MapOrderConfirmationEndpoints();

app.Run();
