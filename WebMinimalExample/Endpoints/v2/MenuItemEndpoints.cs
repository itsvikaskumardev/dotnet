using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebMinimalExample.Data;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Models.DTOs.Item;

namespace WebMinimalExample.Endpoints.v2
{
    public static class MenuItemEndpoints
    {
        public static void MapMenuItemEndpointsV2(this IEndpointRouteBuilder app)
        {
            var api = app.NewVersionedApi("MenuItems");
            var menuItemGroup = api.MapGroup("/apiv{version:apiVersion}/menuitems").WithGroupName("v2").WithTags("MenuItems").MapToApiVersion(new ApiVersion(2, 0));//.RequireAuthorization();

            menuItemGroup.MapGet("/", GetAllMenuItems)
                     .WithName("GetAllMenuItemsV2")
                     .Produces<ApiResponse>(StatusCodes.Status200OK)
                     .MapToApiVersion(new ApiVersion(2, 0))
                     .ProducesProblem(StatusCodes.Status500InternalServerError);
        }

        private static async Task<IResult> GetAllMenuItems(ApplicationDbContext db, IMapper mapper)
        {
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = new
                {
                    Message = "this is v2 for menu items API",
                    Version = "2.0",
                    NewFeatures = new[]
                    {
                        "Support for nutritional information",
                        "Allergen warning",
                        "Special dietary flag - vegan, gluten-free etc",
                        "Chef recommendation"
                    },
                    Demo = "future version can add these features without breaking v1 clients"
                },
                StatusCode = HttpStatusCode.OK
            });
        }
    }
}