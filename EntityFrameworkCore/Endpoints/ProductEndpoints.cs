using EntityFrameworkCore.Data;
using EntityFrameworkCore.Dtos;
using EntityFrameworkCore.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkCore.Endpoints
{
    public static class ProductEndpoints
    {
        public static void MapProductEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/products").WithTags("Products");

            // Create Product
            group.MapPost("/", async (CreateProductRequest request, ApplicationDbContext db) =>
            {
                var product = new Product
                {
                    Id = Guid.NewGuid(),
                    ProductName = request.ProductName,
                    SKU = request.SKU,
                    Price = request.Price
                };

                db.Products.Add(product);
                await db.SaveChangesAsync();

                return Results.Created($"/api/products/{product.Id}", product);
            });

            // Get All Products
            group.MapGet("/", async (ApplicationDbContext db) =>
            {
                var products = await db.Products
                    .AsNoTracking()
                    .Select(p => new ProductResponseDto
                    {
                        ProductName = p.ProductName,
                        SKU = p.SKU,
                        CurrentPrice = p.Price
                    })
                    .ToListAsync();

                return Results.Ok(products);
            });

            // Get Product By Id
            group.MapGet("/{id:guid}", async (Guid id, ApplicationDbContext db) =>
            {
                var product = await db.Products.FindAsync(id);
                if (product is null)
                    return Results.NotFound(new { message = "Product not found" });

                return Results.Ok(product);
            });
        }
    }

    public record CreateProductRequest(string ProductName, string SKU, decimal Price);
}
