namespace impulse_spending_tracker.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public decimal MonthlyNetIncome { get; set; }
        public int RiskToleranceScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<Purchase> Purchases { get; set; } = new();
        public List<SpendingSession> Sessions { get; set; } = new();
        public List<WishlistItem> WishlistItems { get; set; } = new();
        public List<BudgetPlan> BudgetPlans { get; set; } = new();
    }
}