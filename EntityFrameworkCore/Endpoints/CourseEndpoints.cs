using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;

namespace EntityFrameworkCore.Endpoints
{
    public static class CourseEndpoints
    {
        public static void MapCourseEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/courses").WithTags("Courses");

            group.MapPost("/add", async (Course course, ApplicationDbContext db) =>
            {
                db.Courses.Add(course);

                await db.SaveChangesAsync();

                return Results.Created($"/courses/{course.Id}", course);
            });

        }
    }
}
