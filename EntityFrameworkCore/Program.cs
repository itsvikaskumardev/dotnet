using EntityFrameworkCore.Data;
using EntityFrameworkCore.Data.Interceptors;
using EntityFrameworkCore.Endpoints;
using EntityFrameworkCore.Interfaces;
using EntityFrameworkCore.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// 1. HTTP Context Accessor
builder.Services.AddHttpContextAccessor();

// 2. Current User Service
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// 3. Register EF Core Interceptor
builder.Services.AddScoped<AuditableInterceptor>();

// 4. DbContext Set Up + Interceptor
builder.Services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
    options
        .UseLazyLoadingProxies()
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(serviceProvider.GetRequiredService<AuditableInterceptor>()));

/*
Object Cycle Error:
The problem occurs because Student -> Department -> Students -> Department creates an infinite object loop when ASP.NET Core converts the EF Core entities to JSON.

Solution: Tell the JSON serializer to ignore circular references:
*/
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// OpenAPI + Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var url = app.Urls.FirstOrDefault();

        if (url != null)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = $"{url}/scalar",
                UseShellExecute = true
            });
        }
    });
}

// Map Endpoint Modules
app.MapStudentEndpoints();
app.MapDepartmentEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();
app.MapEnrollmentEndpoints();
app.MapUserEndpoints();
app.MapWmsOrderEndpoints();

app.UseHttpsRedirection();

app.Run();
