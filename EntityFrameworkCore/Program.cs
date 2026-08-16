using EntityFrameworkCore.Data;
using EntityFrameworkCore.Endpoints;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

//--- Db Set Up
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.MapStudentEndpoints();
app.MapDepartmentEndpoints();
app.MapTeacherEndpoints();
app.MapCourseEndpoints();
app.MapEnrollmentEndpoints();

app.UseHttpsRedirection();

app.Run();