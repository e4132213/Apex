using System.ComponentModel.DataAnnotations;

namespace Apex.Venues.Models
{
    public class VenueDto
    {
        [Key, MaxLength(5)]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Range(1, Int32.MaxValue)]
        public int Capacity { get; set; }

        public static VenueDto FromModel(Data.Venue venue)
        {
            return new VenueDto
            {
                Code = venue.Code,
                Name = venue.Name,
                Description = venue.Description,
                Capacity = venue.Capacity
            };
        }


    }
}
