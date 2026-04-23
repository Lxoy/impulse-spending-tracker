using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public decimal MonthlyNetIncome { get; set; }
        public int RiskToleranceScore { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public virtual ICollection<SpendingSession> Sessions { get; set; } = new List<SpendingSession>();
        public virtual ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public virtual ICollection<BudgetPlan> BudgetPlans { get; set; } = new List<BudgetPlan>();
    }
}