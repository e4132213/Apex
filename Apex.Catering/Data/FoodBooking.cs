using System.ComponentModel.DataAnnotations;

namespace Apex.Catering.Data
{
    // Represents a catering booking made by a client for one or more guests.
    // Used by EF Core for persistence.
    public class FoodBooking
    {
        // Identifier for the specific booking being made
        // Primary key for the FoodBooking table.
        // The [Key] attribute tells EF Core this is the primary key.
        [Key]
        public int FoodBookingId { get; set; }

        // Identifier referencing the client who made the booking.
        public int ClientReferenceId { get; set; }

        // Number of guests included in the booking. 
        [Range(1, 10000)]
        public int NumberOfGuests { get; set; }

        // Identifier of the selected menu for this booking.
        public int MenuId { get; set; }

        // Make navigation optional so the PUT schema does NOT require full menu details.
        public Menu? Menus { get; set; }
    }
}