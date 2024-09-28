using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class UtilitiesModel
    {
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
