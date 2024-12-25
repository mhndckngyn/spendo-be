namespace Spendo.Models
{
    public class Income
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public decimal Money { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public int AccountId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

}
