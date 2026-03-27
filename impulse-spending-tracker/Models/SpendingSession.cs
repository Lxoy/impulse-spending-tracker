namespace impulse_spending_tracker.Models
{
    public class SpendingSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserProfileId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public decimal SessionBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public int ItemsViewed { get; set; }
        public int ItemsAddedToCart { get; set; }
        public bool CheckoutCompleted { get; set; }

        public UserProfile? UserProfile { get; set; }
        public List<Purchase> Purchases { get; set; } = new();
    }
}