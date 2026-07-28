using System.ComponentModel.DataAnnotations;

namespace WebMinimalExample.Models.DTOs
{
    public class CategoryCreateDTO
    {
     
        [Required(ErrorMessage = "Category name is required ")]
        [StringLength(30, ErrorMessage = "Category name cannot exceed 30 characters")]
        public required string Name { get; set; }

       
    }
}
