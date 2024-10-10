using BE_EXE201.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class HomeModel
    {
        public int? HomeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public float? Price { get; set; }
        public float? Size { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Bathroom { get; set; }

        public int? Bedrooms { get; set; }

        // Location details
        [Required]
        [MaxLength(50)]
        public string Province { get; set; }

        [Required]
        [MaxLength(50)]
        public string District { get; set; }

        [MaxLength(50)]
        public string Town { get; set; }

        [MaxLength(50)]
        public string Street { get; set; }

        [MaxLength(50)]
        public string? HouseNumber { get; set; }

        // Utilities details
        public bool? Elevator { get; set; }
        public bool? SwimmingPool { get; set; }
        public bool? Gym { get; set; }
        public bool? TV { get; set; }
        public bool? Refrigerator { get; set; }
        public bool? Parking { get; set; }
        public bool? Balcony { get; set; }
        public bool? AirConditioner { get; set; }
        // public ICollection<HomeImageModel> HomeImages { get; set; } = new List<HomeImageModel>();

    }
}
