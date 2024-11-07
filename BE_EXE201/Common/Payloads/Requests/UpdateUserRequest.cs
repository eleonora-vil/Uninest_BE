using BE_EXE201.Dtos;
using BE_EXE201.Helpers;
using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Common.Payloads.Requests
{
    public class UpdateUserRequest
    {
        public string? UserName { get; set; }

        [MaxLength(100)]
        public string? Password { get; set; }

        [MaxLength(50)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(100)]
        public string? Gender { get; set; }

       
        [MaxLength(255)]
        public string? Address { get; set; }

        public DateTime? BirthDate { get; set; }

        [MaxLength(15)]
        public string? PhoneNumber { get; set; }
        public string? Status { get; set; }

        public bool? IsActiveMember { get; set; }
        public bool? IsMember { get; set; }
        public decimal? Wallet { get; set; }


    }

    /* public static class UpdateUserRequestExtenstion
     {
         public static UserModel ToUserModel(this UpdateUserRequest req)
         {
             var userModel = new UserModel()
             {
                 UserName = req.UserName,
                 FullName = req.FullName,
                 Gender = req.Gender,
                 Address = req.Address,
                 CreateDate = DateTime.Now,
                 Email = req.Email,
                 PhoneNumber = req.PhoneNumber,
                 Status = req.Status
             };

             if (!string.IsNullOrEmpty(req.Password))
             {
                 userModel.Password = SecurityUtil.Hash(req.Password);
             }

             // Kiểm tra và thiết lập ngày sinh
             if (req.BirthDate.HasValue)
             {
                 userModel.BirthDate = req.BirthDate.Value;
             }
             else
             {
                 userModel.BirthDate = DateTime.Now;
             }

             return userModel;
         }
     }*/
}
