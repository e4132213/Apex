using System.ComponentModel.DataAnnotations;

namespace Apex.Catering.Data
{
    public class MenuFoodItem
    {
        [Required]
        public int MenuId { get; set; }
        public required int FoodItemId { get; set; }
        public Menu Menus { get; set; } = null!;
        public FoodItem FoodItems { get; set; } = null!;
    }
}