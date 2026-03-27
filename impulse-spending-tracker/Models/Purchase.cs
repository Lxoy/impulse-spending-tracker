namespace impulse_spending_tracker.Models
{
    public class Purchase
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserProfileId { get; set; }
        public Guid MerchantId { get; set; }
        public Guid? SpendingSessionId { get; set; }
        public Guid? BudgetPlanId { get; set; }
        public Guid? WishlistItemId { get; set; }

        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime PurchasedAt { get; set; }
        public string MoodBeforePurchase { get; set; } = string.Empty;
        public int NeedLevel { get; set; }
        public ImpulseTriggerType TriggerType { get; set; }
        public int Installments { get; set; }
        public string? Notes { get; set; }

        public UserProfile? UserProfile { get; set; }
        public Merchant? Merchant { get; set; }
        public SpendingSession? SpendingSession { get; set; }
        public BudgetPlan? BudgetPlan { get; set; }
        public WishlistItem? WishlistItem { get; set; }
        public List<Tag> Tags { get; set; } = new();
    }
}