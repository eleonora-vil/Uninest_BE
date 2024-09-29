using Microsoft.CodeAnalysis;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace BE_EXE201.Entities
{
        [Table("PaymentTransaction")]
    public class PaymentTransaction
    {
        public int Id { get; set; }
        public string OrderId { get; set; }  // This is the OrderId from VNPay
        public int UserId { get; set; }       // Foreign key to associate with User
        public decimal Amount { get; set; }   // Payment amount
        public string TransactionId { get; set; }  // VNPay TransactionId
        public string Status { get; set; }    // Transaction status
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public User User { get; set; }
    }
}
