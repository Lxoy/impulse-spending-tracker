namespace impulse_spending_tracker.Models
{
    public class WishlistItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserProfileId { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal DesiredPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public int Priority { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? TargetPurchaseDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsPurchased { get; set; }
        public string LinkUrl { get; set; } = string.Empty;

        public UserProfile? UserProfile { get; set; }
        public Purchase? ConvertedPurchase { get; set; }
    }
}