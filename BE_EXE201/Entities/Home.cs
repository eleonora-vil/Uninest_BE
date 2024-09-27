using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("Home")]
    public class Home
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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

        [ForeignKey("ImageId")]
        public Image? Image { get; set; }

        [ForeignKey("LocationId")]
        public Location? Location { get; set; }

        [ForeignKey("UtilitiesId")]
        public Utilities? Utilities { get; set; }
    }
}
