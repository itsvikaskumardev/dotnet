using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebMinimalExample.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        // Foreign Key column in the MenuItems table.
        // This stores the Id of the related Category.
        // Example:
        // Category Table:
        // Id = 1, Name = "Pizza"
        //
        // MenuItem Table:
        // Id = 10, Name = "Margherita", CategoryId = 1
        public int CategoryId { get; set; }

        // Navigation Property
        //
        // A navigation property allows Entity Framework Core to navigate
        // from one entity to its related entity using the foreign key.
        //
        // CategoryId stores only the integer value (e.g., 1),
        // but this property gives access to the complete Category object.
        //
        // Example:
        // var menuItem = await _db.MenuItems
        //     .Include(m => m.Category)
        //     .FirstAsync();
        //
        // Console.WriteLine(menuItem.Category?.Name);
        // Output: Pizza
        //
        // Without this navigation property, you would only have:
        // menuItem.CategoryId = 1
        // and would need another query to retrieve the Category details.
        //
        // The '?' means this property can be null if the related Category
        // has not been loaded by Entity Framework.
        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}