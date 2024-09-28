using BE_EXE201.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class HomeModel
    {
        public int HomeId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public float? Price { get; set; }
        public float? Size { get; set; }

        [MaxLength(255)]
        public string? Description { get; set; }

        public int? Bathroom { get; set; }

        public int? Bedrooms { get; set; }

        public string? HouseStatus { get; set; }

        public string? Status { get; set; }

        public DateTimeOffset? CreateDate { get; set; }

        public DateTimeOffset? ModifyDate { get; set; }

        public string? CreateBy { get; set; }

        public string? ModifyBy { get; set; }

        public string? ApproveStatus { get; set; }

        public int? ImageId { get; set; }

        public int? LocationId { get; set; }

        public int? UtilitiesId { get; set; }

        public Image? Image { get; set; }

        public Location? Location { get; set; }

        public Utilities? Utilities { get; set; }
    }
}
