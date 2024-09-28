using BE_EXE201.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class ReportModel
    {
        public int ReportId { get; set; }

        public int HomeId { get; set; }

        public Home? Home { get; set; }

        [MaxLength(255)]
        public string? ReportReason { get; set; }

        public string? ReportedBy { get; set; }

        public DateTimeOffset? ReportDate { get; set; }

        public bool? IsResolved { get; set; }
    }
}
