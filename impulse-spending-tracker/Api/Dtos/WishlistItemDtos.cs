using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Api
{
    public class WishlistItemSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal DesiredPrice { get; set; }
    }

    public sealed class WishlistItemDto : WishlistItemSummaryDto
    {
        public UserProfileSummaryDto? UserProfile { get; set; }
        public decimal CurrentPrice { get; set; }
        public int Priority { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? TargetPurchaseDate { get; set; }
        public string? Reason { get; set; }
        public bool IsPurchased { get; set; }
        public string? LinkUrl { get; set; }
        public PurchaseSummaryDto? ConvertedPurchase { get; set; }
    }

    public sealed class WishlistItemUpsertDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        [Required(ErrorMessage = "Item name is required.")]
        [StringLength(140, MinimumLength = 2, ErrorMessage = "Item name must be between 2 and 140 characters.")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 1000000, ErrorMessage = "Desired price must be greater than 0.")]
        public decimal DesiredPrice { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Current price must be greater than 0.")]
        public decimal CurrentPrice { get; set; }

        [Range(1, 5, ErrorMessage = "Priority must be between 1 and 5.")]
        public int Priority { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? AddedAt { get; set; }

        [DataType(DataType.Date)]
        public DateTime? TargetPurchaseDate { get; set; }

        [StringLength(500, ErrorMessage = "Reason can have up to 500 characters.")]
        public string? Reason { get; set; }

        public bool IsPurchased { get; set; }

        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300, ErrorMessage = "Link URL can have up to 300 characters.")]
        public string? LinkUrl { get; set; }
    }
}