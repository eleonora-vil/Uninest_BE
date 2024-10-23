namespace BE_EXE201.Dtos
{
    public class RecentTransactionModel
    {
        public string TransactionId { get; set; }
        public string Username { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public DateTime CreateDate { get; set; }


    }
}
