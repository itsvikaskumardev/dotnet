using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebMinimalExample.Data;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;


namespace WebMinimalExample.Endpoints
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categoryGroup = app.MapGroup("/api/categories");

            categoryGroup.MapGet("/", GetAllCategories)
                 .WithName("GetAllCategories")
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .ProducesProblem(StatusCodes.Status500InternalServerError);

            categoryGroup.MapGet("/{id:int}", GetCategoryById)
                   .WithName("GetCategoryById")
                   .Produces<ApiResponse>(StatusCodes.Status200OK)
                   .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                   .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            categoryGroup.MapPost("", CreateCategory)
                 .WithName("CreateCategory")
                 .Produces<ApiResponse>(StatusCodes.Status201Created)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


            categoryGroup.MapPut("/{id:int}", UpdateCategory)
                 .WithName("UpdateCategory")
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


            categoryGroup.MapDelete("/{id:int}", DeleteCategory)
                 .WithName("DeleteCategory")
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


        }


        private static async Task<IResult> GetAllCategories(ApplicationDbContext db)
        {
            var categories = await db.Categories.ToListAsync();
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = categories,
                StatusCode = HttpStatusCode.OK
            });
        }


        private static async Task<IResult> GetCategoryById(int id, ILogger<Program> logger, ApplicationDbContext db)
        {
            logger.LogInformation("Retrieving category with ID:{CategoryId}", id);

            var category = await db.Categories.FindAsync(id);
            if (category is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = category,
                StatusCode = HttpStatusCode.OK
            });
        }


        private static async Task<IResult> CreateCategory(
            CategoryCreateDTO categoryCreateDTO,
            ApplicationDbContext db,
            IMapper mapper)
        {
            var category = mapper.Map<Category>(categoryCreateDTO);

            category.AddedDate = DateTime.UtcNow;

            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();

            var categoryDTO = mapper.Map<CategoryDTO>(category);

            return Results.Created($"/api/categories/{category.Id}", new ApiResponse
            {
                IsSuccess = true,
                Result = categoryDTO,
                StatusCode = HttpStatusCode.Created
            });
        }



        private static async Task<IResult> UpdateCategory(int id, CategoryUpdateDTO categoryUpdateDTO, ApplicationDbContext db, IMapper mapper)
        {
            var category = await db.Categories.FindAsync(id);

            if (category is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }

            mapper.Map(categoryUpdateDTO, category);

            await db.SaveChangesAsync();

            var categoryDTO = mapper.Map<CategoryDTO>(category);

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = categoryDTO,
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> DeleteCategory(int id, ApplicationDbContext db)
        {
            var category = await db.Categories.FindAsync(id);

            if (category is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["Category not found"]
                });
            }

            db.Categories.Remove(category);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "Category deleted successfully"
            });
        }
    }
}