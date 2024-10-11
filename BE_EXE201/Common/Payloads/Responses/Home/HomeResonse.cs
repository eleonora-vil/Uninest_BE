using BE_EXE201.Dtos;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Common.Payloads.Responses.Home
{
    public class HomeResonse
    {
        public int? HomeId { get; set; }
        public int? UserId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public string? Price { get; set; }
        public string? Size { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Bathroom { get; set; }

        public int? Bedrooms { get; set; }

        // Include full Location details
        public LocationModel Location { get; set; } = new LocationModel();

        // Include full Utilities details
        public UtilitiesModel Utilities { get; set; } = new UtilitiesModel();

        public ICollection<HomeImageModel> HomeImages { get; set; } = new List<HomeImageModel>();
    }

}
