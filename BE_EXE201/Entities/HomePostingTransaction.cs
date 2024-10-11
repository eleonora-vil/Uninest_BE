using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BE_EXE201.Entities
{
    [Table("HomePostingTransaction")]
    public class HomePostingTransaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }  // Foreign key to User
        public User User { get; set; }

        [Required]
        public int HomeId { get; set; }  // Foreign key to Home
        public Home Home { get; set; }

        public decimal Amount { get; set; }  // Amount deducted from the wallet

        public DateTime TransactionDate { get; set; }

        public string Status { get; set; }  // Transaction status (Success, Pending, Failed)
    }

}
