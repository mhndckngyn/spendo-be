namespace Spendo.Models
{
    public class Transaction
    {
        public int ID { get; set; }
        public decimal Amount { get; set; }
        public string Note { get; set; }
        public DateTime TransactionDate { get; set; }
        public int AccountID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
