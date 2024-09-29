namespace BE_EXE201.Dtos.Payment
{
    public class VnPaymentRequestModel
    {
        public int OrderId { get; set; }
        public int UserId { get; set; } // Add UserId to the request
     
        public string Description { get; set; }
        public float Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
