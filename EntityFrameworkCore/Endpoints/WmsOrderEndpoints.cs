using EntityFrameworkCore.Data;
using EntityFrameworkCore.Dtos;
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class WmsOrderEndpoints
    {
        public static void MapWmsOrderEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/orders").WithTags("WmsOrders");

            // 1. Create Order
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

            // 2. Add Item to Order
            group.MapPost("/{orderId:guid}/items", async (
                Guid orderId,
                AddOrderItemRequest request,
                ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders.FindAsync(orderId);
                if (order is null)
                    return Results.NotFound(new { message = "Order not found" });

                var product = await context.Products.FindAsync(request.ProductId);
                if (product is null)
                    return Results.NotFound(new { message = "Product not found" });

                var item = new WmsOrderItem
                {
                    Id = Guid.NewGuid(),
                    WmsOrderId = orderId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = request.UnitPrice > 0 ? request.UnitPrice : product.Price
                };

                context.WmsOrderItems.Add(item);
                await context.SaveChangesAsync();

                return Results.Created($"/api/orders/{orderId}/items/{item.Id}", item);
            });

            // 3. Update Order Status
            group.MapPut("/{id:guid}", async (
                Guid id,
                UpdateOrderRequest request,
                ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders.FindAsync(id);

                if (order is null)
                    return Results.NotFound(new { message = "Order not found" });

                order.Status = request.Status;

                await context.SaveChangesAsync();

                return Results.Ok(order);
            });

            // 4. Get All Orders
            group.MapGet("/", async (ApplicationDbContext context) =>
            {
                var orders = await context.WmsOrders
                    .AsNoTracking()
                    .ToListAsync();

                return Results.Ok(orders);
            });

            // 5. Get Order By Id
            group.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders.FindAsync(id);

                if (order is null)
                    return Results.NotFound(new { message = "Order not found" });

                return Results.Ok(order);
            });

            // 6. Get Order By Id using Projection
            group.MapGet("/projection/{id:guid}", async (
                Guid id,
                ApplicationDbContext dbContext) =>
            {
                var order = await dbContext.WmsOrders
                    .AsNoTracking()
                    .Where(o => o.Id == id)
                    .Select(o => new WmsOrderResponseDto
                    {
                        Id = o.Id,
                        OrderNumber = o.OrderNumber,
                        Status = o.Status,

                        Items = o.Items.Select(i => new WmsOrderItemResponseDto
                        {
                            ProductName = i.Product.ProductName,
                            SKU = i.Product.SKU,
                            Quantity = i.Quantity,
                            UnitPrice = i.UnitPrice,
                            TotalAmount = i.Quantity * i.UnitPrice
                        }).ToList(),

                        /*
                         Yahan ek important concept: Tumne Include() nahi lagaya,
                        phir bhi ProductName aur SKU mil rahe hain. 
                        Kyunki EF Core Select() ke andar navigation properties ko SQL mein translate kar sakta hai. 
                        Projection mein related data lene ke liye Include() zaroori nahi hota.
                         */

                        OrderTotal = o.Items.Sum(i => i.Quantity * i.UnitPrice)
                    })
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    return Results.NotFound(new
                    {
                        message = "WMS Order not found"
                    });
                }

                return Results.Ok(order);
            })
            .WithName("GetWmsOrderByIdProjection")
            .WithSummary("Get WMS Order by ID using Projection");

            // 7. Get Order By Id Using Include & ThenInclude
            group.MapGet("/with-include/{id:guid}", async (
                Guid id,
                ApplicationDbContext dbContext) =>
            {
                var order = await dbContext.WmsOrders
                    .AsNoTracking()
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return Results.NotFound(new
                    {
                        message = "WMS Order not found"
                    });
                }

                var response = new WmsOrderResponseDto
                {
                    Id = order.Id,
                    OrderNumber = order.OrderNumber,
                    Status = order.Status,

                    Items = order.Items.Select(i => new WmsOrderItemResponseDto
                    {
                        ProductName = i.Product.ProductName,
                        SKU = i.Product.SKU,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalAmount = i.Quantity * i.UnitPrice
                    }).ToList(),

                    OrderTotal = order.Items.Sum(i => i.Quantity * i.UnitPrice)
                };

                return Results.Ok(response);
            })
            .WithName("GetWmsOrderByIdWithInclude")
            .WithSummary("Get WMS Order by ID using Include");
        }
    }

    public record CreateOrderRequest(string OrderNumber);
    public record UpdateOrderRequest(string Status);
    public record AddOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice = 0);
}
