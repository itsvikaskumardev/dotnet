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

            // 8. Get Order By Id Using LINQ Query Syntax
            group.MapGet("/query-syntax/{id:guid}", async (
                Guid id,
                ApplicationDbContext context) =>
            {
                var order = await (
                    from o in context.WmsOrders.AsNoTracking()
                    where o.Id == id
                    select new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Status,
                        o.CreatedDate,
                        Items = (
                            from oi in context.WmsOrderItems
                            join p in context.Products on oi.ProductId equals p.Id
                            where oi.WmsOrderId == o.Id
                            select new
                            {
                                oi.Id,
                                oi.Quantity,
                                oi.UnitPrice,
                                Product = new
                                {
                                    p.Id,
                                    p.ProductName,
                                    p.SKU,
                                    p.Price
                                }
                            }
                        ).ToList()
                    }
                ).FirstOrDefaultAsync();

                if (order is null)
                {
                    return Results.NotFound(new { message = "Order not found" });
                }

                return Results.Ok(order);
            })
            .WithName("GetWmsOrderByQuerySyntax")
            .WithSummary("Get WMS Order by ID using LINQ Query Syntax");

            // 9. Get Order By Id Using LINQ Method Syntax
            group.MapGet("/method-syntax/{id:guid}", async (
                Guid id,
                ApplicationDbContext context) =>
            {
                var order = await context.WmsOrders
                    .AsNoTracking()
                    .Where(o => o.Id == id)
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Status,
                        o.CreatedDate,
                        Items = context.WmsOrderItems
                            .Where(oi => oi.WmsOrderId == o.Id)
                            .Join(
                                context.Products,
                                oi => oi.ProductId,
                                p => p.Id,
                                (oi, p) => new
                                {
                                    oi.Id,
                                    oi.Quantity,
                                    oi.UnitPrice,
                                    Product = new
                                    {
                                        p.Id,
                                        p.ProductName,
                                        p.SKU,
                                        p.Price
                                    }
                                }
                            )
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (order is null)
                {
                    return Results.NotFound(new { message = "Order not found" });
                }

                return Results.Ok(order);
            })
            .WithName("GetWmsOrderByMethodSyntax")
            .WithSummary("Get WMS Order by ID using LINQ Method Syntax");

            // 10. Get Orders Using IQueryable (Database-side filtering)
            group.MapGet("/queryable", async (
                ApplicationDbContext context) =>
            {
                // Query database level par build hoti hai (SQL WHERE clause generate hota hai)
                IQueryable<WmsOrder> query = context.WmsOrders
                    .AsNoTracking()
                    .Where(o => o.Status == "Pending");

                var result = await query
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Status,
                        Items = o.Items.Select(oi => new
                        {
                            oi.Id,
                            oi.Quantity,
                            oi.UnitPrice,
                            Product = new
                            {
                                oi.Product.Id,
                                oi.Product.ProductName,
                                oi.Product.SKU,
                                oi.Product.Price
                            }
                        }).ToList()
                    })
                    .ToListAsync();

                return Results.Ok(result);
            })
            .WithName("GetOrdersUsingIQueryable")
            .WithSummary("Get Pending Orders using IQueryable (Database-side Filter)");

            // 11. Get Orders Using IEnumerable (In-Memory filtering)
            group.MapGet("/ienumerable", async (
                ApplicationDbContext context) =>
            {
                // Step 1: Database se data fetch karo
                var orders = await context.WmsOrders
                    .AsNoTracking()
                    .Include(o => o.Items)
                        .ThenInclude(oi => oi.Product)
                    .ToListAsync();

                // Step 2: Data ab memory mein available hai
                IEnumerable<WmsOrder> query = orders;

                // Step 3: In-memory filtering
                var result = query
                    .Where(o => o.Status == "Pending")
                    .Select(o => new
                    {
                        o.Id,
                        o.OrderNumber,
                        o.Status,
                        Items = o.Items.Select(oi => new
                        {
                            oi.Id,
                            oi.Quantity,
                            oi.UnitPrice,
                            Product = new
                            {
                                oi.Product.Id,
                                oi.Product.ProductName,
                                oi.Product.SKU,
                                oi.Product.Price
                            }
                        }).ToList()
                    })
                    .ToList();

                return Results.Ok(result);
            })
            .WithName("GetOrdersUsingIEnumerable")
            .WithSummary("Get Pending Orders using IEnumerable (In-Memory Filter)");
        }
    }

    public record CreateOrderRequest(string OrderNumber);
    public record UpdateOrderRequest(string Status);
    public record AddOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice = 0);
}
