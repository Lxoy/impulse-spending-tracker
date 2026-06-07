using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Api
{
    public class SpendingSessionSummaryDto
    {
        public int Id { get; set; }
        public DateTime StartedAt { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
    }

    public sealed class SpendingSessionDto : SpendingSessionSummaryDto
    {
        public UserProfileSummaryDto? UserProfile { get; set; }
        public DateTime? EndedAt { get; set; }
        public decimal SessionBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public int ItemsViewed { get; set; }
        public int ItemsAddedToCart { get; set; }
        public bool CheckoutCompleted { get; set; }
    }

    public sealed class SpendingSessionUpsertDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        public DateTime StartedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? EndedAt { get; set; }

        [Required(ErrorMessage = "Platform is required.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Platform must be between 2 and 40 characters.")]
        public string Platform { get; set; } = string.Empty;

        [Required(ErrorMessage = "Channel is required.")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Channel must be between 2 and 40 characters.")]
        public string Channel { get; set; } = string.Empty;

        [Range(0, 1000000, ErrorMessage = "Session budget cannot be negative.")]
        public decimal SessionBudget { get; set; }

        [Range(0, 1000000, ErrorMessage = "Spent amount cannot be negative.")]
        public decimal SpentAmount { get; set; }

        [Range(0, 1000000, ErrorMessage = "Items viewed cannot be negative.")]
        public int ItemsViewed { get; set; }

        [Range(0, 1000000, ErrorMessage = "Items added to cart cannot be negative.")]
        public int ItemsAddedToCart { get; set; }

        public bool CheckoutCompleted { get; set; }
    }
}