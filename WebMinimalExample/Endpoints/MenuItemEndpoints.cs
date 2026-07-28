using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WebMinimalExample.Data;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;
using WebMinimalExample.Models.DTOs.Item;

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

        private static async Task<IResult> GetAllMenuItems(ApplicationDbContext db, IMapper mapper)
        {
            var menuItems = await db.MenuItems.Include(u => u.Category).ToListAsync();
            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = mapper.Map<IEnumerable<MenuItemDTO>>(menuItems),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> GetMenuItemById(int id, ApplicationDbContext db, IMapper mapper)
        {
            var menuItem = await db.MenuItems.Include(u => u.Category).FirstOrDefaultAsync(u => u.Id == id);
            if (menuItem is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = mapper.Map<MenuItemDTO>(menuItem),
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> CreateMenuItem(
            MenuItemCreateDTO menuItemCreateDTO,
            ApplicationDbContext db,
            IMapper mapper)
        {
            var category = await db.Categories.FindAsync(menuItemCreateDTO.CategoryId);
            if (category is null)
            {
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid Category ID"]
                });
            }

            var menuItem = mapper.Map<MenuItem>(menuItemCreateDTO);
            menuItem.CreatedDate = DateTime.UtcNow;

            await db.MenuItems.AddAsync(menuItem);
            await db.SaveChangesAsync();

            menuItem.Category = category;
            var menuItemDTO = mapper.Map<MenuItemDTO>(menuItem);

            return Results.Created($"/api/menuitems/{menuItem.Id}", new ApiResponse
            {
                IsSuccess = true,
                Result = menuItemDTO,
                StatusCode = HttpStatusCode.Created
            });
        }

        private static async Task<IResult> UpdateMenuItem(
            int id,
            MenuItemUpdateDTO menuItemUpdateDTO,
            ApplicationDbContext db,
            IMapper mapper)
        {
            var menuItem = await db.MenuItems.Include(u => u.Category).FirstOrDefaultAsync(u => u.Id == id);
            if (menuItem is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }

            var category = await db.Categories.FindAsync(menuItemUpdateDTO.CategoryId);
            if (category is null)
            {
                return Results.BadRequest(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.BadRequest,
                    ErrorMessages = ["Invalid Category ID"]
                });
            }

            mapper.Map(menuItemUpdateDTO, menuItem);
            await db.SaveChangesAsync();

            menuItem.Category = category;
            var menuItemDTO = mapper.Map<MenuItemDTO>(menuItem);

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                Result = menuItemDTO,
                StatusCode = HttpStatusCode.OK
            });
        }

        private static async Task<IResult> DeleteMenuItem(int id, ApplicationDbContext db)
        {
            var menuItem = await db.MenuItems.FindAsync(id);
            if (menuItem is null)
            {
                return Results.NotFound(new ApiResponse
                {
                    IsSuccess = false,
                    StatusCode = HttpStatusCode.NotFound,
                    ErrorMessages = ["MenuItem not found"]
                });
            }

            db.MenuItems.Remove(menuItem);
            await db.SaveChangesAsync();

            return Results.Ok(new ApiResponse
            {
                IsSuccess = true,
                StatusCode = HttpStatusCode.OK,
                Result = "MenuItem deleted successfully"
            });
        }
    }
}