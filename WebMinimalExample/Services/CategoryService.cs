using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WebMinimalExample.Data;
using WebMinimalExample.Models;
using WebMinimalExample.Models.DTOs;

namespace WebMinimalExample.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ApplicationDbContext db, IMapper mapper, ILogger<CategoryService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync()
        {
            var categories = await _db.Categories.ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
        }

        public async Task<CategoryDTO?> GetCategoryByIdAsync(int id)
        {
            _logger.LogInformation("Retrieving category with ID:{CategoryId}", id);

            var category = await _db.Categories.FindAsync(id);
            if (category is null)
            {
                return null;
            }

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO categoryCreateDTO)
        {
            var category = _mapper.Map<Category>(categoryCreateDTO);
            category.AddedDate = DateTime.UtcNow;

            await _db.Categories.AddAsync(category);
            await _db.SaveChangesAsync();

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<CategoryDTO?> UpdateCategoryAsync(int id, CategoryUpdateDTO categoryUpdateDTO)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category is null)
            {
                return null;
            }

            _mapper.Map(categoryUpdateDTO, category);
            await _db.SaveChangesAsync();

            return _mapper.Map<CategoryDTO>(category);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category is null)
            {
                return false;
            }

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
