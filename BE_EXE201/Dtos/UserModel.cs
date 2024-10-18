using BE_EXE201.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class UserModel
    {
        public int UserId { get; set; }

        [MaxLength(255)]
        public string UserName { get; set; }

        [MaxLength(100)]
        public string Password { get; set; }

        [MaxLength(50)]
        public string FullName { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Gender { get; set; }

        public string? OTPCode { get; set; }

        [MaxLength(255)]
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }

        [MaxLength(15)]
        public string PhoneNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string Status { get; set; }

        public int RoleID { get; set; }

        public string RoleName { get; set; }

        [MaxLength(255)]
        public string AvatarUrl { get; set; }

        public decimal? Wallet { get; set; }
        public bool? IsMember { get; set; }

        public DateTime? MembershipStartDate { get; set; }

        public DateTime? MembershipEndDate { get; set; }

        public bool? AutoRenewMembership { get; set; }

        [NotMapped]
        public bool IsActiveMember => IsMember == true && MembershipEndDate >= DateTime.Now;
    }
}
