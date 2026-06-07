using System.ComponentModel.DataAnnotations;
using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Api
{
    public class PurchaseSummaryDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime PurchasedAt { get; set; }
    }

    public sealed class PurchaseDto : PurchaseSummaryDto
    {
        public UserProfileSummaryDto? UserProfile { get; set; }
        public MerchantSummaryDto? Merchant { get; set; }
        public SpendingSessionSummaryDto? SpendingSession { get; set; }
        public BudgetPlanSummaryDto? BudgetPlan { get; set; }
        public WishlistItemSummaryDto? WishlistItem { get; set; }
        public string MoodBeforePurchase { get; set; } = string.Empty;
        public int NeedLevel { get; set; }
        public ImpulseTriggerType TriggerType { get; set; }
        public int Installments { get; set; }
        public string? Notes { get; set; }
        public List<TriggerTypeSummaryDto> TriggerTypes { get; set; } = new();
    }

    public sealed class PurchaseUpsertDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A merchant must be selected.")]
        public int MerchantId { get; set; }

        public int? SpendingSessionId { get; set; }
        public int? BudgetPlanId { get; set; }
        public int? WishlistItemId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(140, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 140 characters.")]
        public string Title { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        [StringLength(8, MinimumLength = 3, ErrorMessage = "Currency must have between 3 and 8 characters.")]
        public string Currency { get; set; } = "EUR";

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

        public List<int> TriggerTypeIds { get; set; } = new();
    }
}