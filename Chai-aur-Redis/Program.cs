using Chai_aur_Redis.Data;
using Chai_aur_Redis.OTP_FLow_TTL;
using Chai_aur_Redis.SetupLocal.Endpoints;
using Chai_aur_Redis.SetupLocal.Services;
using Chai_aur_Redis.SetupLocal.Services.Interfaces;
using Chai_aur_Redis.SiteBanner.Endpoints;
using Chai_aur_Redis.User_Json_Hash;
using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map Minimal API Endpoints
app.MapDatabaseEndpoints();
app.MapSiteBannerEndpoints();
app.MapOTPFlowEndpoints();
app.MapUserEndpoints();

app.Run();
