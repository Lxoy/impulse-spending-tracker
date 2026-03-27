namespace impulse_spending_tracker.Models
{
    public class Merchant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public bool IsOnlineOnly { get; set; }
        public int? AverageDeliveryDays { get; set; }

        public List<Purchase> Purchases { get; set; } = new();
    }
}