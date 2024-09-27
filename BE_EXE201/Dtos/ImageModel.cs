using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class ImageModel
    {
        public int ImageId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ImageUrl { get; set; }
    }
}
