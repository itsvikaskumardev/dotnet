using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
                     .DisableAntiforgery()
                     //.RequireAuthorization(u => u.RequireRole(StaticDetails.AdminRole))
                     .Produces<ApiResponse>(StatusCodes.Status201Created)
                     .Produces<ApiResponse>(StatusCodes.Status400BadRequest)
                     .Produces<ApiResponse>(StatusCodes.Status404NotFound)
                     .Produces<ApiResponse>(StatusCodes.Status500InternalServerError);

            menuItemGroup.MapPut("/{id:int}", UpdateMenuItem)
                     .WithName("UpdateMenuItem")
                     .DisableAntiforgery()
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

        //---------------------------------------------------------------------------------------------------------
        /*
         
        ---------------------------
         
         1. FirstOrDefaultAsync():
        
        It returns the first record matching the condition.
        var menuItem = await db.MenuItems .FirstOrDefaultAsync(u => u.Id == id);
         ie.
        SELECT TOP 1 * FROM MenuItems WHERE Id = 2;
        ---------------------------
         
        2.ToListAsync() :
        
        It fetches all records from the table and returns them as a List.
        var menuItems = await db.MenuItems.ToListAsync();
        SELECT * FROM MenuItems; Return Array of Objects

        ---------------------------

        3. FindAsync():

        It finds a record using its Primary Key.

        var category = await db.Categories.FindAsync(menuItemCreateDTO.CategoryId);
        
        ---------------------------
        4. Include(): 
        
        Categories: 
        | Id | Name   |
        | -- | ------ |
        | 1  | Pizza  |
        | 2  | Drinks |

        MenuItems

        | Id | Name       | CategoryId |
        | -- | ---------- | ---------- |
        | 1  | Margherita | 1          |
        | 2  | Coke       | 2          |


      -> Without Include: var menuItems = await db.MenuItems.ToListAsync();
        I got: 
        MenuItem

        Id = 1
        Name = Margherita
        CategoryId = 1

        Category = null

        -----------------------------------

     -> With Inclue: .Include(m => m.Category)
         EF core performs a join :

        SELECT *
        FROM MenuItems
        LEFT JOIN Categories
        ON MenuItems.CategoryId = Categories.Id

        I got :
        MenuItem

        Id = 1

        Name = Margherita

        CategoryId = 1

        Category
            Id = 1
            Name = Pizza

        ---------------------------

        | Method                           | Purpose                                        | Returns if not found |
| -------------------------------- | ---------------------------------------------- | -------------------- |
| `ToListAsync()`                  | Gets all matching records                      | Empty list (`[]`)    |
| `FirstOrDefaultAsync(condition)` | Gets the first matching record                 | `null`               |
| `FindAsync(primaryKey)`          | Finds a record by primary key                  | `null`               |
| `Include()`                      | Loads related entities (navigation properties) | Not applicable       |

       
         5. Step 1: AddAsync(menuItem): 
        This does not insert anything into the database immediately.
        
        It tells EF Core:
        "Track this new menuItem. I want to insert it into the database when I save."
        
        Step 2: SaveChangesAsync()
         Now EF Core looks at everything it is tracking.
        
        It will generate sql like : 
        
        INSERT INTO MenuItems(Name, Price, CategoryId)
        VALUES ('Pasta', 220, 1); 
        and excute it 
         
         */
        //---------------------------------------------------------------------------------------------------------------------

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
            [FromForm] MenuItemCreateDTO menuItemCreateDTO,
            IFormFile? formFile,
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

            if (formFile is not null && formFile.Length > 0)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "menuitems");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                string filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await formFile.CopyToAsync(stream);

                menuItem.ImageUrl = $"/images/menuitems/{fileName}";
            }

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
            [FromForm] MenuItemUpdateDTO menuItemUpdateDTO,
            IFormFile? formFile,
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

            if (formFile is not null && formFile.Length > 0)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                string uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "menuitems");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // Delete old image file if it exists and is local
                if (!string.IsNullOrEmpty(menuItem.ImageUrl))
                {
                    string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", menuItem.ImageUrl.TrimStart('/'));
                    if (File.Exists(oldFilePath))
                    {
                        File.Delete(oldFilePath);
                    }
                }

                string filePath = Path.Combine(uploadPath, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await formFile.CopyToAsync(stream);

                menuItem.ImageUrl = $"/images/menuitems/{fileName}";
            }

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