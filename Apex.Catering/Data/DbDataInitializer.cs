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
            // Ensure database and schema exist. EnsureCreated returns true when the DB/schema was created.
            var created = _context.Database.EnsureCreated();

            // If the database already existed, don't re-seed.
            if (!created)
            {
                return;
            }

            // Seed initial FoodItems
            var foodItems = new List<FoodItem>
            {
                new FoodItem { FoodItemId = 0,  Description = "Chicken Sandwich", UnitPrice = 5.99m },
                new FoodItem { FoodItemId = 1,  Description = "Veggie Wrap", UnitPrice = 4.99m },
                new FoodItem { FoodItemId = 2,  Description = "Caesar Salad", UnitPrice = 6.49m }
            };
            _context.FoodItems.AddRange(foodItems);
            _context.SaveChanges();

            var menus = new List<Menu>
            {
                new Menu { MenuId = 0, MenuName = "Standard Menu" },
                new Menu { MenuId = 1, MenuName = "Vegetarian Menu" }
            };
            _context.Menus.AddRange(menus);
            _context.SaveChanges();

            var menuFoodItems = new List<MenuFoodItem>
            {
                new MenuFoodItem { MenuId = 0, FoodItemId = 0 },
                new MenuFoodItem { MenuId = 0, FoodItemId = 1 },
                new MenuFoodItem { MenuId = 0, FoodItemId = 2 },
                new MenuFoodItem { MenuId = 1, FoodItemId = 1 },
                new MenuFoodItem { MenuId = 1, FoodItemId = 2 }
            };
            _context.MenuFoodItems.AddRange(menuFoodItems);
            _context.SaveChanges();

            var bookings = new List<FoodBooking>
            {
                new FoodBooking { FoodBookingId = 0, MenuId = 0, NumberOfGuests = 10 },
                new FoodBooking { FoodBookingId = 1, MenuId = 1, NumberOfGuests = 5 }
            };
            _context.FoodBookings.AddRange(bookings);
            _context.SaveChanges();
        }

    }
}
