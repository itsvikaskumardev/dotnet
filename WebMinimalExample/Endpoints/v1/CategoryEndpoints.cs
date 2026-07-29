using Asp.Versioning;
using System.Net;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Services;


namespace WebMinimalExample.Endpoints.v1
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categoryGroup = app.MapGroup("/apiv{version:apiVersion}/categories").WithTags("Categories").MapToApiVersion(new ApiVersion(1, 0));//.RequireAuthorization();

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
                 //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                 .Produces<ApiResponse>(StatusCodes.Status201Created)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


            categoryGroup.MapPut("/{id:int}", UpdateCategory)
                 .WithName("UpdateCategory")
                 //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


            categoryGroup.MapDelete("/{id:int}", DeleteCategory)
                 .WithName("DeleteCategory")
                 .RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                 .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                 .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);


        }


        private static async Task<IResult> GetAllCategories(ICategoryService categoryService)
        {
            var categories = await categoryService.GetAllCategoriesAsync();
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = categories,
                StatusCode = HttpStatusCode.OK
            });
        }


        private static async Task<IResult> GetCategoryById(int id, ICategoryService categoryService)
        {
            var category = await categoryService.GetCategoryByIdAsync(id);
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
            ICategoryService categoryService)
        {
            var categoryDTO = await categoryService.CreateCategoryAsync(categoryCreateDTO);

            return Results.Created($"/api/categories/{categoryDTO.Id}", new ApiResponse
            {
                IsSuccess = true,
                Result = categoryDTO,
                StatusCode = HttpStatusCode.Created
            });
        }



        private static async Task<IResult> UpdateCategory(
            int id,
            CategoryUpdateDTO categoryUpdateDTO,
            ICategoryService categoryService)
        {
            var categoryDTO = await categoryService.UpdateCategoryAsync(id, categoryUpdateDTO);

            if (categoryDTO is null)
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
                Result = categoryDTO,
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> DeleteCategory(int id, ICategoryService categoryService)
        {
            var deleted = await categoryService.DeleteCategoryAsync(id);

            if (!deleted)
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
                StatusCode = HttpStatusCode.OK,
                Result = "Category deleted successfully"
            });
        }
    }
}