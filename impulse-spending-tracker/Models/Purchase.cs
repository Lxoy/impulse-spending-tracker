using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(Merchant))]
        public int MerchantId { get; set; }

        [ForeignKey(nameof(SpendingSession))]
        public int? SpendingSessionId { get; set; }

        [ForeignKey(nameof(BudgetPlan))]
        public int? BudgetPlanId { get; set; }

        [ForeignKey(nameof(WishlistItem))]
        public int? WishlistItemId { get; set; }

        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public DateTime PurchasedAt { get; set; }
        public string MoodBeforePurchase { get; set; } = string.Empty;
        public int NeedLevel { get; set; }
        public ImpulseTriggerType TriggerType { get; set; }
        public int Installments { get; set; }
        public string? Notes { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual Merchant? Merchant { get; set; }
        public virtual SpendingSession? SpendingSession { get; set; }
        public virtual BudgetPlan? BudgetPlan { get; set; }
        public virtual WishlistItem? WishlistItem { get; set; }
        public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
    }
}