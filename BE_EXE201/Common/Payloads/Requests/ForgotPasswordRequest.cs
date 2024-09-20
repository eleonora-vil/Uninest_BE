using System.ComponentModel.DataAnnotations;

namespace BE_EXE201.Common.Payloads.Requests
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
