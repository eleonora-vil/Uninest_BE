using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Dtos
{
    public class UserRoleModel
    {
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; }
    }
}
