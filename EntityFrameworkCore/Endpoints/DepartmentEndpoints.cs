using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;

namespace EntityFrameworkCore.Endpoints
{
    public static class DepartmentEndpoints
    {
        public static void MapDepartmentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/departments").WithTags("Departments");

            group.MapPost("/", async (Department department, ApplicationDbContext db) =>
            {
                db.Departments.Add(department);
                await db.SaveChangesAsync();

                return Results.Created($"/departments/{department.Id}", department);
            });
        }
    }
}

/*
 app.MapGet("/students", async (ApplicationDbContext db) =>
            {
                var students = await db.Students.ToListAsync();
                return Results.Ok(students);


            });
 */