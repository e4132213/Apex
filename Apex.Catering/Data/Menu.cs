using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apex.Catering.Data
{
    // Represents a food menu offered by Apex Catering.
    // Used by EF Core for persistence.
    public class Menu
    {
        // Identifier for the specific menu.
        // Primary key for the Menu table.
        // The [Key] attribute tells EF Core this is the primary key.
        [Key]
        public int MenuId { get; set; }

        // Display name of the menu. 
        // - [Required] enforces a non-null value for validation and EF Core model validation.
        // - [MaxLength(50)] limits the maximum length to 50 characters (also used by EF Core when creating the column).
        // - [Column(TypeName = "nvarchar(50)")] instructs EF Core to create an nvarchar(50) column in SQL Server.
        // The property is non-nullable and initialized to an empty string to satisfy the compiler with nullable reference types enabled,
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string MenuName { get; set; } = string.Empty;

        // Navigation: many-to-many join entries for this menu
        public ICollection<MenuFoodItem> MenuFoodItems { get; set; } = new List<MenuFoodItem>();

        // Navigation: bookings that reference this menu
        public ICollection<FoodBooking> FoodBookings { get; set; } = new List<FoodBooking>();
    }
}