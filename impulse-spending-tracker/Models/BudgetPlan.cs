using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class BudgetPlan
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        public int UserProfileId { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public decimal MonthlyLimit { get; set; }
        public double ImpulseCapPercentage { get; set; }
        public decimal EssentialCategoryLimit { get; set; }
        public decimal DiscretionaryCategoryLimit { get; set; }
        public bool IsActive { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Purchase> CoveredPurchases { get; set; } = new List<Purchase>();
    }
}