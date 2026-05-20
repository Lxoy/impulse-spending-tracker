using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class BudgetPlan : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        [Range(1, int.MaxValue, ErrorMessage = "A user must be selected.")]
        public int UserProfileId { get; set; }

        [Required(ErrorMessage = "Plan name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Plan name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? ValidFrom { get; set; }

        [DataType(DataType.Date)]
        public DateTime? ValidTo { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Monthly limit must be greater than 0.")]
        public decimal MonthlyLimit { get; set; }

        [Range(0, 100, ErrorMessage = "Impulse cap percentage must be between 0 and 100.")]
        public double? ImpulseCapPercentage { get; set; }

        [Range(0, 1000000, ErrorMessage = "Essential category limit cannot be negative.")]
        public decimal? EssentialCategoryLimit { get; set; }

        [Range(0, 1000000, ErrorMessage = "Discretionary category limit cannot be negative.")]
        public decimal? DiscretionaryCategoryLimit { get; set; }
        public bool? IsActive { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Purchase> CoveredPurchases { get; set; } = new List<Purchase>();
        public bool IsDeleted { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Compare only dates so time parts don't make validation fail unintentionally
            if (ValidFrom.HasValue && ValidTo.HasValue && ValidTo.Value.Date < ValidFrom.Value.Date)
            {
                yield return new ValidationResult(
                    "Valid To date must be on or after Valid From date.",
                    new[] { nameof(ValidTo), nameof(ValidFrom) });
            }

            // Treat null limits as zero for the combined check
            var essential = EssentialCategoryLimit ?? 0m;
            var discretionary = DiscretionaryCategoryLimit ?? 0m;
            if ((essential + discretionary) > MonthlyLimit)
            {
                yield return new ValidationResult(
                    "Essential and discretionary limits combined cannot exceed monthly limit.",
                    new[] { nameof(EssentialCategoryLimit), nameof(DiscretionaryCategoryLimit), nameof(MonthlyLimit) });
            }
        }
    }
}