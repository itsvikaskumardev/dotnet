using WebMinimalExample.Models.DTOs;

namespace WebMinimalExample.Services
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO?> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryCreateDTO);
        Task<CategoryDTO?> UpdateCategoryAsync(int id, CategoryUpdateDTO categoryUpdateDTO);
        Task<bool> DeleteCategoryAsync(int id);
    }
}

/*

Format : Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();

Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync();
│                    │                 │                │
│                    │                 │                └── End of declaration
│                    │                 └────────────────── Method name
│                    └──────────────────────────────────── Returns a collection of CategoryDTO
└───────────────────────────────────────────────────────── Async return type



------------------------------IEnumerable ---------------------------------------
 
 IEnumerable<T> is an interface in C# that represents a collection of objects that can be iterated (looped through).

In your code: IEnumerable<CategoryDTO>

It means: This method returns a collection (list) of CategoryDTO objects.


Why not return List<CategoryDTO>?

You could write: public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
                  or
public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()

Both work. The difference is that IEnumerable<T> is more flexible.

If you return: IEnumerable<CategoryDTO>

the caller only knows that it can read and iterate over the collection.

If you return: List<CategoryDTO>

the caller knows it's specifically a List and can do things like:

categories.Add(new CategoryDTO());
categories.RemoveAt(0);

Returning IEnumerable<T> hides the implementation details and is generally preferred when the caller only needs to read the data.


 * -------------------------------------------------------------------------------------------------------------

 */