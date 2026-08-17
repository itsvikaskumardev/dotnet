using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;

namespace EntityFrameworkCore.Endpoints
{
    public static class EnrollmentEndpoints
    {
        public static void MapEnrollmentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/enrollments").WithTags("Enrollments");

            group.MapPost("/add", async (Enrollment enrollment, ApplicationDbContext db) =>
            {
                enrollment.EnrollmentDate = enrollment.EnrollmentDate.ToUniversalTime();
                db.Enrollments.Add(enrollment);

                await db.SaveChangesAsync();

                return Results.Created(
                    $"/enrollments/{enrollment.Id}",
                    enrollment);
            });

        }
    }
}
