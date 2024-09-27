using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("User")]
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string? OTPCode { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public string Address { get; set; }
        public DateTime BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string Status { get; set; }
        public int RoleID { get; set; }
        public virtual UserRole UserRole { get; set; }
        public string AvatarUrl { get; set; }

        public float? Wallet { get; set; }
    }
}
