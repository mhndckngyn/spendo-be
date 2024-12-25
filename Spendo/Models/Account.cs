namespace Spendo.Models
{
    public class Account
    {
        public int ID { get; set; }
        public decimal Balance { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
