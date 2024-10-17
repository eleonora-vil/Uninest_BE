using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("User")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [MaxLength(255)]
        public string UserName { get; set; }

        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        public string? OTPCode { get; set; }
        [Required]
        [MaxLength(50)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }

        public DateTime CreateDate { get; set; }

        public string Status { get; set; }

        [ForeignKey("UserRole")]
        public int RoleID { get; set; }
        public virtual UserRole UserRole { get; set; }

        [MaxLength(255)]
        public string? AvatarUrl { get; set; }

        public decimal? Wallet { get; set; }

        public bool? IsMember { get; set; }

        public DateTime? MembershipStartDate { get; set; }

        public DateTime? MembershipEndDate { get; set; }

        public bool? AutoRenewMembership { get; set; }

        [NotMapped]
        public bool IsActiveMember => IsMember == true && MembershipEndDate >= DateTime.Now;
        //public ICollection<Home> Homes { get; set; } = new List<Home>();
    }
}
