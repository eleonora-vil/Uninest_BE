using Net.payOS.Types;

namespace BE_EXE201.Dtos.Payment
{
    public class CreatePaymentLinkRequest
    {
        public string productName { get; set; }
        public string description { get; set; }
        public string returnUrl { get; set; }
        public string cancelUrl { get; set; }
        public int price { get; set; }
        public string buyerName { get; set; }

    }

}
