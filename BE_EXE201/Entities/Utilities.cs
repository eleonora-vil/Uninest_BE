using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("Utilities")]
    public class Utilities
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UtilitiesId { get; set; }
        public bool? Elevator { get; set; }
        public bool? SwimmingPool { get; set; }
        public bool? Gym { get; set; }
        public bool? TV { get; set; }
        public bool? Refrigerator { get; set; }
        public bool? Parking { get; set; }
        public bool? Balcony { get; set; }
        public bool? AirConditioner { get; set; }
    }
}
