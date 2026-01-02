using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Apex.Venues.Data;
using Apex.Venues.Models;

namespace Apex.Venues.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly VenuesDbContext _context;

        public ReservationsController(VenuesDbContext context)
        {
            _context = context;
        }


        // GET: api/Reservations/{reference} - e.g api/Reservations/CRKHL20260211
        [HttpGet("{reference}")]
        public async Task<IActionResult> GetReservation([FromRoute] string reference)
        {
            var reservation = await _context.Reservations.FindAsync(reference);
            if (reservation == null)
            {
                return NotFound();
            }
            var venue = await _context.Venues.FindAsync(reservation.VenueCode);
            if (venue == null) 
            {
                return NotFound();

            }
            return Ok(ReservationGetDto.FromModel(reservation, venue));
        }

        // POST: api/Reservations
        // With body - eg:
        // {
        //    "EventDate": "2026-02-11",
        //    "VenueCode": "CRKHL"
        // }
        [HttpPost]
        public async Task<IActionResult>
        CreateReservation([FromBody] ReservationPostDto reservation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var availability = await _context.Availabilities
                                             .Include(a => a.Reservation)
                                             .FirstOrDefaultAsync(
                                                a => a.Date == reservation.EventDate
                                                     && a.VenueCode == reservation.VenueCode);

            if (availability == null || availability.Reservation != null)
            {
                return BadRequest("Venue is not available on the requested date.");
            }
            var venue = await _context.Venues.FindAsync (reservation.VenueCode);
            availability.Reservation = new Reservation
            {
                Reference = $"{availability.VenueCode}{availability.Date:yyyyMMdd}",
                EventDate = availability.Date,
                VenueCode = availability.VenueCode,
                WhenMade = DateTime.Now
            };
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetReservation",
                                   new { reference = availability.Reservation.Reference },
                                   ReservationGetDto.FromModel(availability.Reservation, venue));
        }

        // DELETE: api/Reservations/{reference} - e.g. api/Reservations//CRKHL20260211
        [HttpDelete("{reference}")]
        public async Task<IActionResult> DeleteReservation([FromRoute] string reference)
        {
            var reservation = await _context.Reservations.FindAsync(reference);
            if (reservation == null)
            {
                return NotFound();
            }
            var venue = await _context.Venues.FindAsync(reservation.VenueCode);

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            

            return Ok(ReservationGetDto.FromModel(reservation, venue));
        }
    }
}