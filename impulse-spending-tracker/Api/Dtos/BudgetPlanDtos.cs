using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Api
{
    public class BudgetPlanSummaryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal MonthlyLimit { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class BudgetPlanDto : BudgetPlanSummaryDto
    {
        public UserProfileSummaryDto? UserProfile { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public double? ImpulseCapPercentage { get; set; }
        public decimal? EssentialCategoryLimit { get; set; }
        public decimal? DiscretionaryCategoryLimit { get; set; }
    }

    public sealed class BudgetPlanUpsertDto
    {
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

        public bool IsActive { get; set; }
    }
}