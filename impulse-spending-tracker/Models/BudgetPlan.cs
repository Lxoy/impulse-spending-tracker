namespace impulse_spending_tracker.Models
{
    public class BudgetPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserProfileId { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public decimal MonthlyLimit { get; set; }
        public double ImpulseCapPercentage { get; set; }
        public decimal EssentialCategoryLimit { get; set; }
        public decimal DiscretionaryCategoryLimit { get; set; }
        public bool IsActive { get; set; }

        public UserProfile? UserProfile { get; set; }
        public List<Purchase> CoveredPurchases { get; set; } = new();
    }
}