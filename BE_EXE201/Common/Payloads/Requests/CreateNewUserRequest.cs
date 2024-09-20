using BE_EXE201.Dtos;
using BE_EXE201.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Common.Payloads.Requests
{
    public class CreateNewUserRequest
    {

        public string CurrentUserName { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Password { get; set; }

        [Required]
        [MaxLength(50)]
        public string FullName { get; set; }

        [Required]
        [MaxLength(255)]
        public string Email { get; set; }

        [MaxLength(100)]
        public string Gender { get; set; }

        [Required]
        [MaxLength(100)]
        public string Level { get; set; }

        [Required]
        [MaxLength(255)]
        public string Address { get; set; }
        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [MaxLength(15)]
        public string PhoneNumber { get; set; }
        public string Status { get; set; }

    }

    public static class UserRequestExtenstion
    {
        public static UserModel ToUserModel(this CreateNewUserRequest req) 
        {
            var UserModel = new UserModel()
            {
                UserName = req.UserName,
                Password = SecurityUtil.Hash(req.Password),
                FullName = req.FullName,
                Gender = req.Gender,
                Address = req.Address,
                BirthDate = req.BirthDate,
                CreateDate = DateTime.Now,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber,
                Status = req.Status
            };    
            return UserModel;
        }
        public static UserRoleModel ToUserRoleModel(this CreateNewUserRequest req) 
        {
            var userRole = new UserRoleModel()
            {
                RoleName = req.RoleName,
            };
            return userRole;
        }

    }

}
