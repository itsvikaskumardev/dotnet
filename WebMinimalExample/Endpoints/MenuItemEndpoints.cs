using WebMinimalExample.Models;

namespace WebMinimalExample.Endpoints
{
    public static class MenuItemEndpoints
    {
        public static void MapMenuItemEndpoints(this IEndpointRouteBuilder app)
        {
            var menuItemGroup = app.MapGroup("/api/menuitems").WithTags("MenuItems");//.RequireAuthorization();

            menuItemGroup.MapGet("/", GetAllMenuItems)
                     .WithName("GetAllMenuItems")
                     .Produces<ApiResponse>(StatusCodes.Status200OK)
                     .ProducesProblem(StatusCodes.Status500InternalServerError);

            menuItemGroup.MapGet("/{id:int}", GetMenuItemById)
                       .WithName("GetMenuItemById")
                       .Produces<ApiResponse>(StatusCodes.Status200OK)
                       .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                       .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            menuItemGroup.MapPost("", CreateMenuItem)
                     .WithName("CreateMenuItem")
                     //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                     .Produces<ApiResponse>(StatusCodes.Status201Created)
                     .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                     .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                     .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            menuItemGroup.MapPut("/{id:int}", UpdateMenuItem)
                     .WithName("UpdateMenuItem")
                     //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                     .Produces<ApiResponse>(StatusCodes.Status200OK)
                     .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                     .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                     .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            menuItemGroup.MapDelete("/{id:int}", DeleteMenuItem)
                     .WithName("DeleteMenuItem")
                     //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                     .Produces<ApiResponse>(StatusCodes.Status200OK)
                     .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                     .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);
        }
    }
}