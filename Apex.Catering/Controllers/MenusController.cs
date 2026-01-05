using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex.Catering.Data;

namespace Apex.Catering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public MenusController(CateringDbContext context)
        {
            _context = context;
        }

        // GET: api/Menus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Menu>>> Get()
        {
            var menus = await _context.Menus
                .Include(m => m.MenuFoodItems!)
                    .ThenInclude(mfi => mfi.FoodItems!)
                .AsNoTracking()
                .ToListAsync();

            return Ok(menus);
        }

        // GET: api/Menus/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Menu>> Get(int id)
        {
            var menu = await _context.Menus
                .Include(m => m.MenuFoodItems!)
                    .ThenInclude(mfi => mfi.FoodItems!)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MenuId == id);

            if (menu is null) return NotFound();
            return Ok(menu);
        }

        // POST: api/Menus
        [HttpPost]
        public async Task<ActionResult<Menu>> Post([FromBody] Menu model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.MenuId = 0; // ensure identity not supplied
            _context.Menus.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.MenuId }, model);
        }

        // PUT: api/Menus/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] Menu model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != model.MenuId && model.MenuId != 0) return BadRequest("Id mismatch.");

            var existing = await _context.Menus.FindAsync(id);
            if (existing is null) return NotFound();

            // Update allowed fields
            existing.MenuName = model.MenuName;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Menus.AnyAsync(m => m.MenuId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Menus/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.Menus
                .Include(m => m.MenuFoodItems)
                .FirstOrDefaultAsync(m => m.MenuId == id);

            if (existing is null) return NotFound();

            _context.Menus.Remove(existing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Possibly constrained by related records (bookings, menu items)
                return Conflict("Unable to delete Menu because it is referenced by related records. Remove references first.");
            }

            return NoContent();
        }

        // POST: api/Menus/5/items/3  -> add food item to menu
        [HttpPost("{menuId:int}/items/{foodItemId:int}")]
        public async Task<IActionResult> AddFoodItem(int menuId, int foodItemId)
        {
            var menu = await _context.Menus.FindAsync(menuId);
            if (menu is null) return NotFound("Menu not found.");

            var food = await _context.FoodItems.FindAsync(foodItemId);
            if (food is null) return NotFound("FoodItem not found.");

            var exists = await _context.MenuFoodItems.AnyAsync(m => m.MenuId == menuId && m.FoodItemId == foodItemId);
            if (exists) return Conflict("Food item already exists on the menu.");

            var entry = new MenuFoodItem
            {
                MenuId = menuId,
                FoodItemId = foodItemId
            };

            _context.MenuFoodItems.Add(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Menus/5/items/3 -> remove food item from menu
        [HttpDelete("{menuId:int}/items/{foodItemId:int}")]
        public async Task<IActionResult> RemoveFoodItem(int menuId, int foodItemId)
        {
            var entry = await _context.MenuFoodItems
                .FindAsync(menuId, foodItemId);

            if (entry is null) return NotFound();

            _context.MenuFoodItems.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}