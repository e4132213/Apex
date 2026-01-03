namespace Apex.Catering.Data
{
    public class DbDataInitializer
    {
        private readonly CateringDbContext _context;

        public DbDataInitializer(CateringDbContext context)
        {
            _context = context;
        }

        public void InitializeData()
        {
            // Ensure database is created
            _context.Database.EnsureCreated();
            // Check if there are any FoodItems already present
            if (_context.FoodItems.Any())
            {
                return; // Data already initialized
            }
            // Seed initial FoodItems
            var foodItems = new List<FoodItem>
            {
                new FoodItem { FoodItemId = 00,  Description = "Chicken Sandwich", UnitPrice = 5.99m },
                new FoodItem { FoodItemId = 01,  Description = "Veggie Wrap", UnitPrice = 4.99m },
                new FoodItem { FoodItemId = 02,  Description = "Caesar Salad", UnitPrice = 6.49m }
            };
            _context.FoodItems.AddRange(foodItems);
            _context.SaveChanges();

            var menus = new List<Menu>
            {
                new Menu { MenuId = 00, MenuName = "Standard Menu" },
                new Menu { MenuId = 01, MenuName = "Vegetarian Menu" }
            };
            _context.Menus.AddRange(menus);
            _context.SaveChanges();

            var menuFoodItems = new List<MenuFoodItem>
            {
                new MenuFoodItem { MenuId = 00, FoodItemId = 00 },
                new MenuFoodItem { MenuId = 00, FoodItemId = 01 },
                new MenuFoodItem { MenuId = 00, FoodItemId = 02 },
                new MenuFoodItem { MenuId = 01, FoodItemId = 01 },
                new MenuFoodItem { MenuId = 01, FoodItemId = 02 }
            };
            _context.MenuFoodItems.AddRange(menuFoodItems);
            _context.SaveChanges();

            var bookings = new List<FoodBooking>
            {
                new FoodBooking { FoodBookingId = 00, MenuId = 00, NumberOfGuests = 10 },
                new FoodBooking { FoodBookingId = 01, MenuId = 01, NumberOfGuests = 05 }
            };
            _context.FoodBookings.AddRange(bookings);
            _context.SaveChanges();
        }

    }
}
