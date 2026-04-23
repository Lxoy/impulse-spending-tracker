using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Models
{
    public class Merchant
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool IsOnlineOnly { get; set; }
        public int? AverageDeliveryDays { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}