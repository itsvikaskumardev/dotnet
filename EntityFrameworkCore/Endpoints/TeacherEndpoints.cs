using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;

namespace EntityFrameworkCore.Endpoints
{
    public static class TeacherEndpoints
    {
        public static void MapTeacherEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/teachers").WithTags("Teachers");

            group.MapPost("/", async (Teacher teacher, ApplicationDbContext db) =>
            {
                db.Teachers.Add(teacher);
                await db.SaveChangesAsync();
                return Results.Created($"/teachers/{teacher.Id}", teacher);
            });
        }
    }
}
