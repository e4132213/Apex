using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Apex.Venues.Models
{
    public class ReservationPostDto
    {
        [Required, DataType(DataType.Date)]
        public DateTime EventDate { get; set; }

        [Required, MinLength(5), MaxLength(5)]
        public string VenueCode { get; set; }

    }
}
