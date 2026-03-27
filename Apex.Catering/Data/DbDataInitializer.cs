using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
            // Prefer migrations; fall back to EnsureCreated when migrations are not available.
            try
            {
                _context.Database.Migrate();
            }
            catch (Exception)
            {
                _context.Database.EnsureCreated();
            }

            // Seed FoodItems if missing
            if (!_context.FoodItems.Any())
            {
                var foodItems = new List<FoodItem>
                {
                    new FoodItem { Description = "Chicken Sandwich", UnitPrice = 5.99m },
                    new FoodItem { Description = "Veggie Wrap", UnitPrice = 4.99m },
                    new FoodItem { Description = "Caesar Salad", UnitPrice = 6.49m }
                };
                _context.FoodItems.AddRange(foodItems);
                _context.SaveChanges();
            }

            // Seed Menus if missing
            if (!_context.Menus.Any())
            {
                var menus = new List<Menu>
                {
                    new Menu { MenuName = "Standard Menu" },
                    new Menu { MenuName = "Vegetarian Menu" }
                };
                _context.Menus.AddRange(menus);
                _context.SaveChanges();
            }

            // Seed MenuFoodItems if missing (lookup IDs from DB to avoid ID collisions)
            if (!_context.MenuFoodItems.Any())
            {
                var chickenId = _context.FoodItems.First(fi => fi.Description == "Chicken Sandwich").FoodItemId;
                var veggieId = _context.FoodItems.First(fi => fi.Description == "Veggie Wrap").FoodItemId;
                var caesarId = _context.FoodItems.First(fi => fi.Description == "Caesar Salad").FoodItemId;

                var standardMenuId = _context.Menus.First(m => m.MenuName == "Standard Menu").MenuId;
                var vegetarianMenuId = _context.Menus.First(m => m.MenuName == "Vegetarian Menu").MenuId;

                var menuFoodItems = new List<MenuFoodItem>
                {
                    new MenuFoodItem { MenuId = standardMenuId, FoodItemId = chickenId },
                    new MenuFoodItem { MenuId = standardMenuId, FoodItemId = veggieId },
                    new MenuFoodItem { MenuId = standardMenuId, FoodItemId = caesarId },
                    new MenuFoodItem { MenuId = vegetarianMenuId, FoodItemId = veggieId },
                    new MenuFoodItem { MenuId = vegetarianMenuId, FoodItemId = caesarId }
                };

                _context.MenuFoodItems.AddRange(menuFoodItems);
                _context.SaveChanges();
            }

            // Seed FoodBookings if missing
            if (!_context.FoodBookings.Any())
            {
                var standardMenuId = _context.Menus.First(m => m.MenuName == "Standard Menu").MenuId;
                var vegetarianMenuId = _context.Menus.First(m => m.MenuName == "Vegetarian Menu").MenuId;

                var bookings = new List<FoodBooking>
                {
                    new FoodBooking { MenuId = standardMenuId, NumberOfGuests = 10, ClientReferenceId = 0 },
                    new FoodBooking { MenuId = vegetarianMenuId, NumberOfGuests = 5, ClientReferenceId = 0 }
                };

                _context.FoodBookings.AddRange(bookings);
                _context.SaveChanges();
            }
        }
    }
}
