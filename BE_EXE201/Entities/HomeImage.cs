using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("HomeImage")]
    public class HomeImage
    {
        [Key]
        public int HomeImageId { get; set; }

        public int HomeId { get; set; }
        public Home Home { get; set; }

        public int ImageId { get; set; }
        public Image Image { get; set; }

        // Optional additional fields (e.g., description, location, order)
        public string? ImageDescription { get; set; }
    }
}
