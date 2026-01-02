// EF Core core APIs: DbContext, DbSet, DbContextOptions, ModelBuilder, etc.
using Microsoft.EntityFrameworkCore;
// types for dependency resolution (kept as in original file)
using Microsoft.Extensions.DependencyModel.Resolution;

// Declares the namespace for the classes in this file
namespace Apex.Catering.Data
{
    // database context for the catering system.
    // The DbContext implementation that represents the database session and EF Core model
    public class CateringDbContext : DbContext
    {
        // Constructor used when dependency injection provides DbContextOptions (typical in ASP.NET Core)
        public CateringDbContext(DbContextOptions<CateringDbContext> options) : base(options) { }

        // DbSet properties tell EF Core which entity types should be part of the model and allow querying/CRUD
        public DbSet<FoodItem> FoodItems { get; set; } // Represents the FoodItems table
        public DbSet<Menu> Menus { get; set; } // Represents the Menu table
        public DbSet<FoodBooking> FoodBookings { get; set; } // Represents the FoodBooking table
        public DbSet<MenuFoodItem> MenuFoodItems { get; set; } // Represents the many-to-many join table

        // Private field to hold the SQLite database file path when using the parameterless constructor
        private string DbPath { get; set; } = string.Empty;

        // Parameterless constructor initializes DbPath to a location under the current user's local application data
        public CateringDbContext()
        {
            // Select standard folder for local app data
            var folder = Environment.SpecialFolder.LocalApplicationData;
            // Get the actual path string for that folder
            var path = Environment.GetFolderPath(folder);
            // Build the full path to the SQLite file
            DbPath = System.IO.Path.Join(path, "Apex.catering.db");
        }

        // OnConfiguring is called by EF Core to configure options when the context is created without DI options
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Calls base implementation (no-op in DbContext but kept for clarity)
            base.OnConfiguring(optionsBuilder);
            // Configure EF Core to use SQLite with the DbPath file
            optionsBuilder.UseSqlite($"Data Source={DbPath}");
        }

        // OnModelCreating is where model configuration (fluent API) is applied
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Call base to allow any base-class configuration (none by default)
            base.OnModelCreating(builder);

            // Configure composite primary key for entity MenuFoodItem
            builder.Entity<MenuFoodItem>()
                .HasKey(mf => new { mf.MenuId, mf.FoodItemId });

            // Configure relationships and delete behaviors to restrict cascading deletes
            builder.Entity<FoodItem>()
                .HasMany(fi => fi.MenuFoodItems)
                .WithOne(mf => mf.FoodItems)
                .HasForeignKey(mf => mf.FoodItemId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Menu>()
                .HasMany(m => m.MenuFoodItems)
                .WithOne(mf => mf.Menus)
                .HasForeignKey(mf => mf.MenuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial data for FoodItem entity
            builder.Entity<FoodItem>().HasData(
                new FoodItem { FoodItemId = 1, Description = "Chicken Sandwich", UnitPrice = 5.99m },
                new FoodItem { FoodItemId = 2, Description = "Veggie Wrap", UnitPrice = 4.99m },
                new FoodItem { FoodItemId = 3, Description = "Caesar Salad", UnitPrice = 3.99m }
            );
        }
    }
}