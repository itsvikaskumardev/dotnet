using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Channels;

namespace EntityFrameworkCore.Endpoints
{
    public static class StudentEndpoints
    {
        public static void MapStudentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/students").WithTags("Students");


            //Step1: Add ---------------------------------------

            group.MapPost("/add", async (Student student, ApplicationDbContext db) =>
            {
                db.Students.Add(student);// does not immediately insert into PostgreSQL.: EF Core: "Start tracking this Student as a new entity."

                await db.SaveChangesAsync();

                return Results.Created(
                    $"/students/{student.Id}",
                    student);
            });

            // Step2: AddAsync---------------------------------------

            group.MapPost("/add-async", async (Student student, ApplicationDbContext db) =>
            {
                await db.Students.AddAsync(student);

                await db.SaveChangesAsync();

                return Results.Created($"/students/{student.Id}", student);
            });

            // Step3: Update----------------------------------------

            group.MapPut("/update-tracked/{id}", async (
                int id,
                Student request,
                ApplicationDbContext db) =>
            {
                var student = await db.Students.FindAsync(id);// returns a tracked entity.
                //FindAsync()->  Student loaded +tracked

                if (student is null)
                    return Results.NotFound();

                student.Name = request.Name;
                student.Age = request.Age;
                student.Gender = request.Gender;
                student.DepartmentId = request.DepartmentId;
                //Changes its value. EF Core detects the change.


                await db.SaveChangesAsync();

                return Results.Ok(student);
            });

            // Step4: Update()----------------------------------------

            /*
             * Update(): It will Update the whole Entity 
             * 
             * A common situation is when you already have a detached entity and want to tell EF Core:
             * "Treat this entity as an existing entity that needs updating."
             */
            group.MapPut("/update-full/{id}", async (
                int id,
                Student student,
                ApplicationDbContext db) =>
            {
                if (id != student.Id)
                {
                    return Results.BadRequest("ID mismatch.");
                }

                db.Students.Update(student);

                await db.SaveChangesAsync();

                return Results.Ok(student);
            });

            // Step5: ToListAsync() -----------------------------------

            group.MapGet("/all", async (ApplicationDbContext db) =>
            {
                var students = await db.Students.ToListAsync();
                return Results.Ok(students);
            });

            // Step6: AsNoTracking()

            /*
             * AsNoTracking() 
             * 
             * For a read-only query: EF Core doesn't track these entities for later modification.
             * Use it mainly for: GET/read-only operations
             */

            group.MapGet("/no-tracking", async (ApplicationDbContext db) =>
            {
                var students = await db.Students
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(students);
            });

            //Step 7: Skip() + Take()

            /*
             * These are extremely important for pagination.
             * If you have 20 students:
             * 
             * Page 1 → students 1–5
             * Page 2 → students 6–10
             * Page 3 → students 11–15
             * Page 4 → students 16–20
             * 
             * Skip = (page - 1) × pageSize
             */
            group.MapGet("/page/{page}", async (int page, ApplicationDbContext db) =>
            {
                int pageSize = 5;

                var students = await db.Students
                    .OrderBy(s => s.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Results.Ok(students);
            });

            //Step 8 : FirstOrDefaultAsync() : -------------------------------

            /*
             * FirstOrDefaultAsync():
             * Finds the first matching record, or returns null.
             * 
             * When you want to search/filter.
             * Suppose you want: "Find the first student whose age is greater than 20."
             */

            group.MapGet("/first-or-default/{id}", async (int id, ApplicationDbContext db) =>
            {
                //var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
                var student = await db.Students.FirstOrDefaultAsync(s => s.Age > 20);


                if (student == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(student);

            });



            //Step 9: FindAsync()

            /*
             * FindAsync(): When you know the Primary Key.
             * e.g., "Give me the student whose primary key is 5."
             */

            group.MapGet("/find/{id}", async (int id, ApplicationDbContext db) =>
            {
                var student = await db.Students.FindAsync(id);

                if (student == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(student);
            });

            //Step 10: Where()

            group.MapGet("/age/{age}", async (int age, ApplicationDbContext db) =>
            {
                var students = await db.Students
                    .Where(s => s.Age > age)
                    .ToListAsync();

                return Results.Ok(students);
            });

            //Step 11: Select() : Don't return the complete entity:

            group.MapGet("/basic-info", async (ApplicationDbContext db) =>
            {
                var students = await db.Students
                    .Select(s => new
                    {
                        s.Id,
                        s.Name,
                        s.Age
                    })
                    .ToListAsync();

                return Results.Ok(students);
            });

            //Step 12: Include()-

            /*
             * Use .Include() when you want to return the complete entity 
             * (e.g., returning the full Student object and their full Department object).
             */

            group.MapGet("/with-department", async (ApplicationDbContext db) =>
            {
                var students = await db.Students
                   .Include(s => s.Department)
                    .ToListAsync();

                /*
                 * You're telling EF Core:
                 * "Get the students and also load their related Department navigation property."
                 * 
                 * The result contains the Student entity + Department entity.
                 */

                return Results.Ok(students);
            });

            //Step 13 — Join() -LINQ

            /*
             * You're saying:
             * "Combine Students and Departments based on DepartmentId and give me this specific result."
             * 
             * Notice that we're not returning a Student entity with a Department navigation property.
             * We're creating a new result.
             */

            group.MapGet("/student-department", async (ApplicationDbContext db) =>
            {
                var result = await (
                    from s in db.Students
                    join d in db.Departments
                        on s.DepartmentId equals d.Id
                    select new
                    {
                        StudentName = s.Name,
                        DepartmentName = d.Name
                    }
                ).ToListAsync();

                return Results.Ok(result);
            });

           //Step 14: Don't do this
            group.MapGet("/cannot-do-this", async (ApplicationDbContext db) =>
            {
                var result = await db.Students
                    .Include(s => s.Department) // EF Core ignores this!
                    .Select(s => new
                    {
                        StudentName = s.Name,
                        DepartmentName = s.Department.Name
                    })
                    .ToListAsync();

                return Results.Ok(result);
            });

           //Step 15:  
            // and often don't even need Include and Join
            // EF Core understands that Department.Name is needed and generates the appropriate SQL.
            group.MapGet("/no-join-no-include", async (ApplicationDbContext db) =>
            {
                var result = await db.Students
                    .Select(s => new
                    {
                        StudentName = s.Name,
                        DepartmentName = s.Department.Name
                    })
                    .ToListAsync();

                return Results.Ok(result);
            });

            //Step 16: ThenInclude()

            group.MapGet("/with-courses", async (ApplicationDbContext db) =>
            {
                var students = await db.Students
                    .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                    .ToListAsync();

                return Results.Ok(students);
            });

            //Step 17.OrderBy()

            group.MapGet("/ordered", async (ApplicationDbContext db) =>
            {
                var students = await db.Students
                    .OrderBy(s => s.Name)
                    // .OrderByDescending(s => s.Age) // also use order by descending
                    .ToListAsync();

                return Results.Ok(students);
            });


            //Step 18:GroupBy()

            group.MapGet("/group-by-department", async (ApplicationDbContext db) =>
            {
                var result = await db.Students
                    .GroupBy(s => s.DepartmentId)
                    .Select(g => new
                    {
                        DepartmentId = g.Key,
                        StudentCount = g.Count()
                    })
                    .ToListAsync();

                return Results.Ok(result);
            });

            //Step 19.AnyAsync(): 
            /*
             * Use this when you only want to know whether at least one record exists.
             * Return True or False.
             */

            group.MapGet("/exists", async (ApplicationDbContext db) =>
            {
                var exists = await db.Students
                    .AnyAsync();

                return Results.Ok(exists);
            });

            //Step 20. CountAsync(): Use it when you want the number of records.

            group.MapGet("/count", async (ApplicationDbContext db) =>
            {
                var count = await db.Students.CountAsync();

                return Results.Ok(count);
            });

            //Step 21. SumAsync()
            group.MapGet("/total-marks", async (ApplicationDbContext db) =>
            {
                var totalMarks = await db.Enrollments
                    .SumAsync(e => e.Marks);

                return Results.Ok(totalMarks);
            });

            // Step 22: AverageAsync()

            group.MapGet("/average-marks", async (ApplicationDbContext db) =>
            {
                var averageMarks = await db.Enrollments
                    .AverageAsync(e => e.Marks);

                return Results.Ok(averageMarks);
            });

            //Step 23: MinAsync() and MaxAsync()

            group.MapGet("/marks-summary", async (ApplicationDbContext db) =>
            {
                var result = new
                {
                    Minimum = await db.Enrollments.MinAsync(e => e.Marks),
                    Maximum = await db.Enrollments.MaxAsync(e => e.Marks),
                    Average = await db.Enrollments.AverageAsync(e => e.Marks),
                    Total = await db.Enrollments.SumAsync(e => e.Marks)
                };

                return Results.Ok(result);
            });

            //Step 24: Distinct()
            //Suppose you want to know which departments students belong to, without duplicates:

            group.MapGet("/department-ids", async (ApplicationDbContext db) =>
            {
                var departments = await db.Students
                    .Select(s => s.DepartmentId)
                    .Distinct()
                    .ToListAsync();

                return Results.Ok(departments);
            });


        }
    }
}
