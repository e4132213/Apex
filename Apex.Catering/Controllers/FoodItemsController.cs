using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex.Catering.Data;

namespace Apex.Catering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public FoodItemsController(CateringDbContext context)
        {
            _context = context;
        }

        // GET: api/FoodItems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodItem>>> Get()
        {
            return Ok(await _context.FoodItems.AsNoTracking().ToListAsync());
        }

        // GET: api/FoodItems/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FoodItem>> Get(int id)
        {
            var item = await _context.FoodItems.AsNoTracking().FirstOrDefaultAsync(fi => fi.FoodItemId == id);
            if (item is null) return NotFound();
            return Ok(item);
        }

        // POST: api/FoodItems
        [HttpPost]
        public async Task<ActionResult<FoodItem>> Post([FromBody] FoodItem model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Ensure identity is not accidentally supplied
            model.FoodItemId = 0;
            _context.FoodItems.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = model.FoodItemId }, model);
        }

        // PUT: api/FoodItems/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] FoodItem model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != model.FoodItemId && model.FoodItemId != 0) return BadRequest("Id mismatch.");

            var existing = await _context.FoodItems.FindAsync(id);
            if (existing is null) return NotFound();

            // Update allowed fields
            existing.Description = model.Description;
            existing.UnitPrice = model.UnitPrice;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FoodItems.AnyAsync(f => f.FoodItemId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/FoodItems/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.FoodItems.Include(f => f.MenuFoodItems).FirstOrDefaultAsync(f => f.FoodItemId == id);
            if (existing is null) return NotFound();

            _context.FoodItems.Remove(existing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Likely constrained by related MenuFoodItems. Return 409 with message.
                return Conflict("Unable to delete FoodItem because it is referenced by menus. Remove references first.");
            }

            return NoContent();
        }
    }
}