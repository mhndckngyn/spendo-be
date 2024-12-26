using System.ComponentModel.DataAnnotations;

namespace Spendo.Models
{
    public class Currency
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public string Code { get; set; }
        public string Sign { get; set; }
    }
}
