using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(Merchant))]
        [Range(1, int.MaxValue, ErrorMessage = "A merchant must be selected.")]
        public int MerchantId { get; set; }

        [ForeignKey(nameof(SpendingSession))]
        public int? SpendingSessionId { get; set; }

        [ForeignKey(nameof(BudgetPlan))]
        public int? BudgetPlanId { get; set; }

        [ForeignKey(nameof(WishlistItem))]
        public int? WishlistItemId { get; set; }

        [ForeignKey(nameof(TriggerTypeTag))]
        [Range(1, int.MaxValue, ErrorMessage = "A trigger type must be selected.")]
        public int? TriggerTypeId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(140, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 140 characters.")]
        public string Title { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Currency must have between 3 and 8 characters.")]
        public string Currency { get; set; } = "EUR";

        [DataType(DataType.DateTime)]
        public DateTime PurchasedAt { get; set; }

        [StringLength(120, ErrorMessage = "Mood description can have up to 120 characters.")]
        public string MoodBeforePurchase { get; set; } = string.Empty;

        [Range(1, 10, ErrorMessage = "Need level must be between 1 and 10.")]
        public int NeedLevel { get; set; }
        public ImpulseTriggerType TriggerType { get; set; }

        [Range(1, 36, ErrorMessage = "Installments must be between 1 and 36.")]
        public int Installments { get; set; }

        [StringLength(1000, ErrorMessage = "Notes can have up to 1000 characters.")]
        public string? Notes { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual Merchant? Merchant { get; set; }
        public virtual SpendingSession? SpendingSession { get; set; }
        public virtual BudgetPlan? BudgetPlan { get; set; }
        public virtual WishlistItem? WishlistItem { get; set; }
        public virtual TriggerType? TriggerTypeTag { get; set; }
        public virtual ICollection<TriggerType> TriggerTypes { get; set; } = new List<TriggerType>();
        public virtual ICollection<PurchaseAttachment> Attachments { get; set; } = new List<PurchaseAttachment>();
        public bool IsDeleted { get; set; }
    }
}