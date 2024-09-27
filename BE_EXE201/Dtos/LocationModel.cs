using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class LocationModel
    {
        public int LocationId { get; set; }

        [MaxLength(50)]
        public string? Province { get; set; }

        [MaxLength(50)]
        public string? District { get; set; }

        [MaxLength(50)]
        public string? Town { get; set; }

        [MaxLength(50)]
        public string? Street { get; set; }

        [MaxLength(50)]
        public string? HouseNumber { get; set; }
    }
}
