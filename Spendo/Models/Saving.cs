namespace Spendo.Models
{
    public class Saving
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public decimal Goal { get; set; }
        public DateTime SavingDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
