using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Api
{
    public class UserProfileSummaryDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public sealed class UserProfileDto : UserProfileSummaryDto
    {
        public DateTime DateOfBirth { get; set; }
        public decimal MonthlyNetIncome { get; set; }
        public int RiskToleranceScore { get; set; }
    }

    public sealed class UserProfileUpsertDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 60 characters.")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(60, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 60 characters.")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }

        [Range(0.01, 1000000, ErrorMessage = "Monthly income must be greater than 0.")]
        public decimal MonthlyNetIncome { get; set; }

        [Range(1, 10, ErrorMessage = "Risk tolerance score must be between 1 and 10.")]
        public int RiskToleranceScore { get; set; }
    }
}