using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#1F6FEB";
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}