using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apex.Catering.Data
{
    public class FoodItem
    {
        // Identifier for the specific food item.
        // Primary key for the FoodItems table.
        // The [Key] attribute tells EF Core this is the primary key.
        [Key]
        public int FoodItemId { get; set; }

        // readable description of the food item.
        // - [Required] enforces a non-null value for validation and EF Core model validation.
        // - [MaxLength(50)] limits the maximum length to 50 characters (also used by EF Core when creating the column).
        // - [Column(TypeName = "nvarchar(50)")] instructs EF Core to create an nvarchar(50) column in SQL Server.
        // The property is non-nullable and initialized to an empty string to satisfy the compiler with nullable reference types enabled.
        [Required]
        [MaxLength(50)]
        [Column(TypeName = "nvarchar(50)")]
        public string Description { get; set; } = string.Empty;

        // Price per unit of the food item.
        // - Decimal type is used for currency to avoid floating-point precision issues.
        // - the property is non-nullable and initialized to 0 to satisfy the compiler with nullable reference types enabled.
        public decimal UnitPrice { get; set; } = 0;

        // Navigation: many-to-many join entries for this food item
        public ICollection<MenuFoodItem> MenuFoodItems { get; set; } = new List<MenuFoodItem>();
    }
}
