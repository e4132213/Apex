using System.ComponentModel.DataAnnotations;

namespace Apex.Catering.Data
{
    // DTO used for creating/updating FoodBooking from API clients.
    public class FoodBookingDto
    {
        // Optional on create; for PUT the route id is authoritative.
        public int FoodBookingId { get; set; }

        public int ClientReferenceId { get; set; }

        [Range(1, 10000)]
        public int NumberOfGuests { get; set; }

        // Menu selection by id only (no nested Menu graph).
        [Required]
        public int MenuId { get; set; }
    }
}