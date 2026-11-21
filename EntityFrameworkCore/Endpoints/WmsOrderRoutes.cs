namespace EntityFrameworkCore.Endpoints
{
    public static class WmsOrderRoutes
    {
        public static void MapWmsOrderEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/orders").WithTags("WmsOrders");

            // =========================================================================
            // WMS ORDER ENDPOINT ROUTES (INDEX / NAVIGATION)
            // Tip: Press F12 (or Ctrl+Click) on any method below to jump to its code!
            // =========================================================================

            // 1. Create Order
            group.MapPost("/", WmsOrderEndpoints.CreateOrder);

            // 2. Add Item to Order
            group.MapPost("/{orderId:guid}/items", WmsOrderEndpoints.AddOrderItem);

            // 3. Update Order Status
            group.MapPut("/{id:guid}", WmsOrderEndpoints.UpdateOrderStatus);

            // 4. Get All Orders
            group.MapGet("/", WmsOrderEndpoints.GetAllOrders);

            // 5. Get Order By Id
            group.MapGet("/{id:guid}", WmsOrderEndpoints.GetOrderById);

            // 6. Get Order By Id using Projection
            group.MapGet("/projection/{id:guid}", WmsOrderEndpoints.GetOrderByIdProjection)
                .WithName("GetWmsOrderByIdProjection")
                .WithSummary("Get WMS Order by ID using Projection");

            // 7. Get Order By Id Using Include & ThenInclude
            group.MapGet("/with-include/{id:guid}", WmsOrderEndpoints.GetOrderByIdWithInclude)
                .WithName("GetWmsOrderByIdWithInclude")
                .WithSummary("Get WMS Order by ID using Include");

            // 8. Get Order By Id Using LINQ Query Syntax
            group.MapGet("/query-syntax/{id:guid}", WmsOrderEndpoints.GetOrderByQuerySyntax)
                .WithName("GetWmsOrderByQuerySyntax")
                .WithSummary("Get WMS Order by ID using LINQ Query Syntax");

            // 9. Get Order By Id Using LINQ Method Syntax
            group.MapGet("/method-syntax/{id:guid}", WmsOrderEndpoints.GetOrderByMethodSyntax)
                .WithName("GetWmsOrderByMethodSyntax")
                .WithSummary("Get WMS Order by ID using LINQ Method Syntax");

            // 10. Get Orders Using IQueryable (Database-side filtering)
            group.MapGet("/queryable", WmsOrderEndpoints.GetOrdersUsingIQueryable)
                .WithName("GetOrdersUsingIQueryable")
                .WithSummary("Get Pending Orders using IQueryable (Database-side Filter)");

            // 11. Get Orders Using IEnumerable (In-Memory filtering)
            group.MapGet("/ienumerable", WmsOrderEndpoints.GetOrdersUsingIEnumerable)
                .WithName("GetOrdersUsingIEnumerable")
                .WithSummary("Get Pending Orders using IEnumerable (In-Memory Filter)");

            // 12. GroupBy: SKU Report
            group.MapGet("/sku-report", WmsOrderEndpoints.GetSkuReport)
                .WithName("GetSkuReport")
                .WithSummary("Get SKU Summary Report using GroupBy");

            // 13. ToDictionary: SKU Items / Quantity Lookup
            group.MapGet("/quantity-lookup", WmsOrderEndpoints.GetQuantityLookup)
                .WithName("GetQuantityLookup")
                .WithSummary("Get Items by SKU Dictionary using ToDictionary (Supports optional ?sku= query param)");

            group.MapGet("/sku-items", WmsOrderEndpoints.GetQuantityLookup)
                .WithName("GetSkuItems")
                .WithSummary("Get Items by SKU Dictionary using ToDictionary (Supports optional ?sku= query param)");

            // 14. ToHashSet: Product Availability
            group.MapGet("/check-availability", WmsOrderEndpoints.CheckProductAvailability)
                .WithName("CheckProductAvailability")
                .WithSummary("Check Product Availability using ToHashSet");

            // Also available as /api/products/check-availability
            app.MapGet("/api/products/check-availability", WmsOrderEndpoints.CheckProductAvailability)
                .WithTags("Products")
                .WithName("CheckProductAvailabilityAlternate")
                .WithSummary("Check Product Availability using ToHashSet (Alternate route)");
        }
    }
}
