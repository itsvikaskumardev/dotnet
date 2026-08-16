using EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class StudentEndpoints
    {
        public static void MapStudentEndpoints(this WebApplication app)
        {


            app.MapGet("/students", async (ApplicationDbContext db) =>
            {
                var students = await db.Students.ToListAsync();
                return Results.Ok(students);


            });
        }
    }
}
