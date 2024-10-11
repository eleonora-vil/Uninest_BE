namespace BE_EXE201.Dtos.Payment
{
    public class VnPaymentRequestModel
    {
        public string? OrderId { get; set; }
        public int UserId { get; set; } // Add UserId to the request
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreatedDate { get; set; }
       
    }
}
