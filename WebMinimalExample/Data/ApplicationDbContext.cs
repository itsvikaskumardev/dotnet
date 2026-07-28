using Microsoft.EntityFrameworkCore;
using WebMinimalExample.Models;

namespace WebMinimalExample.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<LocalUser> LocalUsers { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    AddedDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Category
                {
                    Id = 2,
                    Name = "Books",
                    AddedDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc)
                }
            );



            modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                Id = 1,
                Name = "Caesar Salad",
                Description = "Fresh romaine lettuce with parmesan cheese and croutons",
                Price = 8.99m,
                CategoryId = 2, // Appetizer
                CreatedDate = new DateTime(2026, 1, 5)
            },
            new MenuItem
            {
                Id = 2,
                Name = "Grilled Chicken",
                Description = "Tender grilled chicken breast with herbs",
                Price = 15.99m,
                CategoryId = 1, // Entree
                CreatedDate = new DateTime(2026, 1, 5)
            },
            new MenuItem
            {
                Id = 3,
                Name = "Chocolate Cake",
                Description = "Rich chocolate cake with vanilla ice cream",
                Price = 6.99m,
                CategoryId = 3, // Desert
                CreatedDate = new DateTime(2026, 1, 5)
            }
        );
        }

    }
}
