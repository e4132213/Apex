using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex.Catering.Data;

namespace Apex.Catering.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FoodBookingsController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public FoodBookingsController(CateringDbContext context)
        {
            _context = context;
        }

        // GET: api/FoodBookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FoodBooking>>> Get()
        {
            var bookings = await _context.FoodBookings
                .Include(fb => fb.Menus)
                .AsNoTracking()
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/FoodBookings/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<FoodBooking>> Get(int id)
        {
            var booking = await _context.FoodBookings
                .Include(fb => fb.Menus)
                .AsNoTracking()
                .FirstOrDefaultAsync(fb => fb.FoodBookingId == id);

            if (booking is null) return NotFound();
            return Ok(booking);
        }

        // GET: api/FoodBookings/5/fooditems
        // Returns the list of FoodItems associated with the Menu referenced by the booking.
        [HttpGet("{id:int}/fooditems")]
        public async Task<ActionResult<IEnumerable<FoodItem>>> GetFoodItems(int id)
        {
            var booking = await _context.FoodBookings
                .AsNoTracking()
                .FirstOrDefaultAsync(fb => fb.FoodBookingId == id);

            if (booking is null) return NotFound();

            var items = await _context.MenuFoodItems
                .AsNoTracking()
                .Where(mfi => mfi.MenuId == booking.MenuId)
                .Include(mfi => mfi.FoodItems)
                .Select(mfi => mfi.FoodItems)
                .Distinct()
                .ToListAsync();

            return Ok(items);
        }

        // POST: api/FoodBookings
        // Creates a new booking and returns the FoodBookingId as confirmation (201 Created).
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] FoodBooking model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Ensure identity is not supplied by client.
            model.FoodBookingId = 0;

            // Validate referenced Menu exists
            var menuExists = await _context.Menus.AnyAsync(m => m.MenuId == model.MenuId);
            if (!menuExists) return BadRequest($"Menu with id {model.MenuId} does not exist.");

            _context.FoodBookings.Add(model);
            await _context.SaveChangesAsync();

            // Return created with the new id in the response body.
            return CreatedAtAction(nameof(Get), new { id = model.FoodBookingId }, new { FoodBookingId = model.FoodBookingId });
        }

        // PUT: api/FoodBookings/5
        // Edit an existing booking. Idempotent update of allowed fields.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] FoodBooking model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != model.FoodBookingId && model.FoodBookingId != 0) return BadRequest("Id mismatch.");

            var existing = await _context.FoodBookings.FindAsync(id);
            if (existing is null) return NotFound();

            // Validate referenced Menu exists if changed
            if (existing.MenuId != model.MenuId)
            {
                var menuExists = await _context.Menus.AnyAsync(m => m.MenuId == model.MenuId);
                if (!menuExists) return BadRequest($"Menu with id {model.MenuId} does not exist.");
            }

            // Update allowed fields
            existing.ClientReferenceId = model.ClientReferenceId;
            existing.NumberOfGuests = model.NumberOfGuests;
            existing.MenuId = model.MenuId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.FoodBookings.AnyAsync(f => f.FoodBookingId == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/FoodBookings/5
        // Cancels (removes) a booking.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _context.FoodBookings.FindAsync(id);
            if (existing is null) return NotFound();

            _context.FoodBookings.Remove(existing);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // Unexpected constraint issues
                return Conflict("Unable to delete booking due to related data. Investigate constraints.");
            }

            return NoContent();
        }
    }
}