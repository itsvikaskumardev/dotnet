using EntityFrameworkCore.Data;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class UserEndpoints
    {
        public static void MapUserEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/users").WithTags("Users");

            group.MapGet("/list-n+1", async (ApplicationDbContext dbContext) =>
            {
                // N+1 query problem
                var users = await dbContext.Users.ToListAsync();

                var result = new List<object>();

                foreach (var user in users)
                {
                    var total = user.Orders.Count;


                    result.Add(new
                    {
                        user.Id,
                        user.Name,
                        UniqueOrderIds = user.Orders.Select(o => o.UniqueOrderId).ToList(),
                        OrderCount = total

                    });
                }

                return result;
            });

            group.MapGet("/list-soln", async (ApplicationDbContext dbContext) =>
            {
                // Solved N+1 query using Eager Loading
                var users = await dbContext.Users
                    .Include(u => u.Orders)
                    .ToListAsync();

                var result = new List<object>();

                foreach (var user in users)
                {
                    var total = user.Orders.Count;

                    result.Add(new
                    {
                        user.Id,
                        user.Name,
                        UniqueOrderIds = user.Orders.Select(o => o.UniqueOrderId).ToList(),
                        OrderCount = total
                    });
                }

                return result;
            });



        }

    }

}

/