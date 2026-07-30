using Asp.Versioning;
using System.Net;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Services;

namespace WebMinimalExample.Endpoints.v2
{
    public static class CategoryEndpoints
    {
        public static void MapCategoryEndpointsV2(this IEndpointRouteBuilder app)
        {
            var api = app.NewVersionedApi("Categories");
            var categoryGroup = api.MapGroup("/apiv{version:apiVersion}/categories").WithGroupName("v2").WithTags("Categories").MapToApiVersion(new ApiVersion(2, 0));//.RequireAuthorization();

            categoryGroup.MapGet("/", GetAllCategories)
                 .WithName("GetAllCategoriesV2")
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