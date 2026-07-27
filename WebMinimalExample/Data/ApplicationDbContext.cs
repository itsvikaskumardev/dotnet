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
        }

    }
}
