using Asp.Versioning;
using System.Net;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Services;

namespace WebMinimalExample.Endpoints.v2
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            var categoryGroup = app.MapGroup("/apiv{version:apiVersion}/categories").WithTags("Categories");//.RequireAuthorization();

            categoryGroup.MapGet("/", GetAllCategories)
                 .WithName("GetAllCategories")
                 .Produces<ApiResponse>(StatusCodes.Status200OK)
                 .MapToApiVersion(new ApiVersion(2, 0))
                 .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllCategories(ICategoryService categoryService)
        {
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = new
                {
                    Message = "this is v2 for category API",
                    Version = "2.0",
                    NewFeatures = new[]
                    {
                        "Support for category images",
                        "Parent-child category hierarchy",
                        "Category-level display order",
                        "Active/inactive status flag"
                    },
                    Demo = "future version can add these features without breaking v1 clients"
                },
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}