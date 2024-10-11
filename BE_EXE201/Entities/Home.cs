using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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

        public string? Price { get; set; }
        public string? Size { get; set; }
        public string? Description { get; set; }
        public int? Bathroom { get; set; }
        public int? Bedrooms { get; set; }

        // Foreign keys for Location and Utilities
        public int? LocationId { get; set; }
        public int? UtilitiesId { get; set; }

        public Location? Location { get; set; }
        public Utilities? Utilities { get; set; }

        // One-to-Many with HomeImage
        public ICollection<HomeImage> HomeImages { get; set; } = new List<HomeImage>();
        
        //[ForeignKey("UserId")]
        public int? UserId { get; set; }
        public User User { get; set; }
    }
}
