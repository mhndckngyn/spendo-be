namespace Spendo.Models
{
    public class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int UserID { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
