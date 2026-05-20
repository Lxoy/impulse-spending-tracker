using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class WishlistItem : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
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
        public DateTime AddedAt { get; set; }

        [DataType(DataType.Date)]
        public DateTime? TargetPurchaseDate { get; set; }

        [Display(Name = "Notes")]
        [StringLength(500, ErrorMessage = "Reason can have up to 500 characters.")]
        public string? Reason { get; set; }
        public bool IsPurchased { get; set; }

        [Display(Name = "Link URL")]
        [Url(ErrorMessage = "Enter a valid URL.")]
        [StringLength(300, ErrorMessage = "Link URL can have up to 300 characters.")]
        public string? LinkUrl { get; set; }
        public bool IsDeleted { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual Purchase? ConvertedPurchase { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TargetPurchaseDate.HasValue && TargetPurchaseDate.Value.Date < AddedAt.Date)
            {
                yield return new ValidationResult(
                    "Target purchase date cannot be before the date item was added.",
                    new[] { nameof(TargetPurchaseDate), nameof(AddedAt) });
            }
        }
    }
}