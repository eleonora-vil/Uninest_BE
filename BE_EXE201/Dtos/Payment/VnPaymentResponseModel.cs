namespace BE_EXE201.Dtos.Payment
{

    public class VnPaymentResponseModel
    {
        public bool Success { get; set; } // Indicates if the transaction was successful
        public string PaymentMethod { get; set; } // The payment method used (e.g., VNPay)
        public string OrderDescription { get; set; } // The description of the order or payment
        public string OrderId { get; set; } // The ID of the order
        public string PaymentId { get; set; } // VNPay's payment ID
        public string TransactionId { get; set; } // Unique transaction ID from VNPay
        public string Token { get; set; } // Security token returned by VNPay
        public string VnPayResponseCode { get; set; } // Response code returned by VNPay (e.g., "00" for success)

        public decimal Amount { get; set; } // The amount paid in the transaction
        public string WalletBalance { get; set; } // Optionally, include the updated wallet balance after payment
        public string? Message {  get; set; } 
    }


}
