using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class SpendingSession : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        [DataType(DataType.DateTime)]
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

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public bool IsDeleted { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndedAt.HasValue && EndedAt.Value < StartedAt)
            {
                yield return new ValidationResult(
                    "End date cannot be before start date.",
                    new[] { nameof(EndedAt), nameof(StartedAt) });
            }

            if (ItemsAddedToCart > ItemsViewed)
            {
                yield return new ValidationResult(
                    "Items added to cart cannot exceed items viewed.",
                    new[] { nameof(ItemsAddedToCart), nameof(ItemsViewed) });
            }
        }
    }
}