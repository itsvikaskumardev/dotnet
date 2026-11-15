using EntityFrameworkCore.Data;
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class WmsOrderEndpoints
    {
        public static void MapWmsOrderEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/orders").WithTags("WmsOrders");

            // Create Order
            group.MapPost("/", async (
                CreateOrderRequest request,
                ApplicationDbContext context) =>
            {
                var order = new WmsOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = request.OrderNumber,
                    Status = "Pending"
                };

                context.WmsOrders.Add(order);

                await context.SaveChangesAsync();

                return Results.Created($"/api/orders/{order.Id}", order);
            });

            // Update Order
            group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateOrderRequest request,
                ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders.FindAsync(id);

                if (order is null)
                    return Results.NotFound("Order not found");

                order.Status = request.Status;

                await context.SaveChangesAsync();

                return Results.Ok(order);
            });

            // Get All Orders
            group.MapGet("/", async (ApplicationDbContext context) =>
            {
                var orders = await context.WmsOrders
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(orders);
            });

            // Get Order By Id
            group.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders.FindAsync(id);

                if (order is null)
                    return Results.NotFound("Order not found");

                return Results.Ok(order);
            });
        }
    }

    public record CreateOrderRequest(string OrderNumber);
    public record UpdateOrderRequest(string Status);
}
