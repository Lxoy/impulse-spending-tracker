namespace impulse_spending_tracker.Models
{
    public class Tag
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#1F6FEB";
        public string Description { get; set; } = string.Empty;

        public List<Purchase> Purchases { get; set; } = new();
    }
}