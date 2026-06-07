using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace impulse_spending_tracker.Models
{
    public class AppUser : IdentityUser
    {
        [Required(ErrorMessage = "OIB is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "OIB must have exactly 11 digits.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "OIB may contain only digits.")]
        public string OIB { get; set; } = string.Empty;

        [Required(ErrorMessage = "JMBG is required.")]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "JMBG must have exactly 13 digits.")]
        [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG may contain only digits.")]
        public string JMBG { get; set; } = string.Empty;

        // Navigation to domain profile (optional 1:1)
        public UserProfile? UserProfile { get; set; }
    }
}