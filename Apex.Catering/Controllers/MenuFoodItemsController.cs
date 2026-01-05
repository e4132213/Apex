using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex.Catering.Data;

namespace Apex.Catering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuFoodItemsController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public MenuFoodItemsController(CateringDbContext context)
        {
            _context = context;
        }

        // GET: api/MenuFoodItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuFoodItem>>> Get()
        {
            var items = await _context.MenuFoodItems
                .Include(mfi => mfi.Menus)
                .Include(mfi => mfi.FoodItems)
                .AsNoTracking()
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/MenuFoodItems/5/3
        [HttpGet("{menuId:int}/{foodItemId:int}")]
        public async Task<ActionResult<MenuFoodItem>> Get(int menuId, int foodItemId)
        {
            var entry = await _context.MenuFoodItems
                .Include(mfi => mfi.Menus)
                .Include(mfi => mfi.FoodItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(mfi => mfi.MenuId == menuId && mfi.FoodItemId == foodItemId);

            if (entry is null) return NotFound();
            return Ok(entry);
        }

        // POST: api/MenuFoodItems
        // Creates a join entry between a Menu and a FoodItem.
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MenuFoodItem model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Validate referenced Menu and FoodItem exist
            var menuExists = await _context.Menus.AnyAsync(m => m.MenuId == model.MenuId);
            if (!menuExists) return BadRequest($"Menu with id {model.MenuId} does not exist.");

            var foodExists = await _context.FoodItems.AnyAsync(f => f.FoodItemId == model.FoodItemId);
            if (!foodExists) return BadRequest($"FoodItem with id {model.FoodItemId} does not exist.");

            // Prevent duplicate
            var exists = await _context.MenuFoodItems.AnyAsync(m => m.MenuId == model.MenuId && m.FoodItemId == model.FoodItemId);
            if (exists) return Conflict("This food item is already associated with the menu.");

            _context.MenuFoodItems.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { menuId = model.MenuId, foodItemId = model.FoodItemId },
                model);
        }

        // DELETE: api/MenuFoodItems/5/3
        [HttpDelete("{menuId:int}/{foodItemId:int}")]
        public async Task<IActionResult> Delete(int menuId, int foodItemId)
        {
            var entry = await _context.MenuFoodItems.FindAsync(menuId, foodItemId);
            if (entry is null) return NotFound();

            _context.MenuFoodItems.Remove(entry);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                return Conflict("Unable to remove association due to database constraints.");
            }

            return NoContent();
        }
    }
}