using EntityFrameworkCore.Data;
using EntityFrameworkCore.Dtos;
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class WmsOrderEndpoints
    {
        // 1. Create Order
        public static async Task<IResult> CreateOrder(
            CreateOrderRequest request,
            ApplicationDbContext context)
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
        }

        // 2. Add Item to Order
        public static async Task<IResult> AddOrderItem(
            Guid orderId,
            AddOrderItemRequest request,
            ApplicationDbContext context)
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
        }

        // 3. Update Order Status
        public static async Task<IResult> UpdateOrderStatus(
            Guid id,
            UpdateOrderRequest request,
            ApplicationDbContext context)
        {
            var order = await context.WmsOrders.FindAsync(id);
            if (order is null)
                return Results.NotFound(new { message = "Order not found" });

            order.Status = request.Status;
            await context.SaveChangesAsync();

            return Results.Ok(order);
        }

        // 4. Get All Orders
        public static async Task<IResult> GetAllOrders(ApplicationDbContext context)
        {
            var orders = await context.WmsOrders
                .AsNoTracking()
                .ToListAsync();

            return Results.Ok(orders);
        }

        // 5. Get Order By Id
        public static async Task<IResult> GetOrderById(Guid id, ApplicationDbContext context)
        {
            var order = await context.WmsOrders.FindAsync(id);
            if (order is null)
                return Results.NotFound(new { message = "Order not found" });

            return Results.Ok(order);
        }

        // 6. Get Order By Id using Projection
        public static async Task<IResult> GetOrderByIdProjection(
            Guid id,
            ApplicationDbContext dbContext)
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
        }

        // 7. Get Order By Id Using Include & ThenInclude
        public static async Task<IResult> GetOrderByIdWithInclude(
            Guid id,
            ApplicationDbContext dbContext)
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
        }

        // 8. Get Order By Id Using LINQ Query Syntax
        public static async Task<IResult> GetOrderByQuerySyntax(
            Guid id,
            ApplicationDbContext context)
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
        }

        // 9. Get Order By Id Using LINQ Method Syntax
        public static async Task<IResult> GetOrderByMethodSyntax(
            Guid id,
            ApplicationDbContext context)
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
        }

        // 10. Get Orders Using IQueryable (Database-side filtering)
        public static async Task<IResult> GetOrdersUsingIQueryable(ApplicationDbContext context)
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
        }

        // 11. Get Orders Using IEnumerable (In-Memory filtering)
        public static async Task<IResult> GetOrdersUsingIEnumerable(ApplicationDbContext context)
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
        }

        // 12. GroupBy: Get SKU Summary Report
        public static async Task<IResult> GetSkuReport(ApplicationDbContext context)
        {
            var items = await context.WmsOrderItems
                .Include(oi => oi.Product)
                .AsNoTracking()
                .ToListAsync();

            var result = items
                .GroupBy(oi => oi.Product.SKU) // oi => oi.Product.SKU - SKU ko grouping key banata hai
                .Select(g => new
                {
                    SKU = g.Key, // here it is SKU
                    TotalQuantity = g.Sum(x => x.Quantity),
                    TotalOrders = g.Select(x => x.WmsOrderId)
                                   .Distinct()
                                   .Count()
                })
                .ToList();

            /*
             **Output:**
             | SKU    | TotalQuantity | TotalOrders |
             | ------ | ------------- | ----------- |
             | SKU001 | 15            | 2           |
             | SKU002 | 3             | 1           |
             | SKU003 | 2             | 1           |
            */

            return Results.Ok(result);
        }

        // 13. ToDictionary: Group Items by SKU and Lookup (Dictionary<string, List<WmsOrderItem>>)
        public static async Task<IResult> GetQuantityLookup(
            string? sku,
            ApplicationDbContext context)
        {
            var items = await context.WmsOrderItems
                .Include(oi => oi.Product)
                .AsNoTracking()
                .ToListAsync();

            // SKU ke according items ko group karke Dictionary me convert karna
            var itemsBySku = items
                .GroupBy(oi => oi.Product.SKU)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(i => new
                    {
                        i.Id,
                        i.WmsOrderId,
                        i.ProductId,
                        ProductName = i.Product.ProductName,
                        i.Quantity,
                        i.UnitPrice,
                        TotalAmount = i.Quantity * i.UnitPrice
                    }).ToList()
                );

            /*
             Concept: Dictionary<string, List<WmsOrderItem>>
             
             Output structure:
             {
                 "SKU001" : [Item1, Item3],
                 "SKU002" : [Item2]
             }

             Safe Lookup using TryGetValue (prevents KeyNotFoundException):
             if (itemsBySku.TryGetValue("SKU001", out var skuItems))
             {
                 foreach (var item in skuItems)
                 {
                     Console.WriteLine(item.Quantity);
                 }
             }
             var directItems = itemsBySku["SKU001"]; // Throws KeyNotFoundException if key is missing
            */

            // Agar specific SKU query param pass kiya ho (e.g. ?sku=SKU-MOUSE-01)
            if (!string.IsNullOrWhiteSpace(sku))
            {
                if (itemsBySku.TryGetValue(sku, out var skuItems))
                {
                    return Results.Ok(new
                    {
                        SKU = sku,
                        Items = skuItems
                    });
                }

                return Results.NotFound(new
                {
                    message = $"SKU '{sku}' not found in order items."
                });
            }

            // Agar koi SKU filter nahi diya toh poora dictionary return hoga
            return Results.Ok(itemsBySku);
        }

        // 14. ToHashSet: Check Product Availability
        public static async Task<IResult> CheckProductAvailability(
            Guid? productId,
            ApplicationDbContext context)
        {
            var productIds = await context.Products
                .Select(p => p.Id)
                .ToListAsync();

            var availableProducts = productIds.ToHashSet();

            // Agar productId query param nahi diya toh first product check karega
            var targetId = productId ?? (productIds.Count > 0 ? productIds[0] : Guid.Parse("10000000-0000-0000-0000-000000000001"));

            bool isAvailable = availableProducts.Contains(targetId);

            return Results.Ok(new
            {
                ProductId = targetId,
                IsAvailable = isAvailable
            });
        }
    }

    public record CreateOrderRequest(string OrderNumber);
    public record UpdateOrderRequest(string Status);
    public record AddOrderItemRequest(Guid ProductId, int Quantity, decimal UnitPrice = 0);
}
