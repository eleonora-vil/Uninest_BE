using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("Report")]
    public class Report
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReportId { get; set; }

        public int? HomeId { get; set; }
        [ForeignKey("HomeId")]
        public Home? Home { get; set; }

        [MaxLength(255)]
        public string? ReportReason { get; set; }

        public string? ReportedBy { get; set; }

        public DateTimeOffset? ReportDate { get; set; }

        public bool? IsResolved { get; set; }
    }
}
