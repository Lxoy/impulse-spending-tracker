using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Models
{
    public class Merchant
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Merchant name is required.")]
        [StringLength(120, MinimumLength = 2, ErrorMessage = "Merchant name must be between 2 and 120 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Category must be between 2 and 80 characters.")]
        public string Category { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country code is required.")]
        [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Country code must contain exactly 2 letters.")]
        public string CountryCode { get; set; } = string.Empty;
        public bool? IsOnlineOnly { get; set; }

        [Range(1, 90, ErrorMessage = "Average delivery days must be between 1 and 90 when provided.")]
        public int? AverageDeliveryDays { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public bool IsDeleted { get; set; }
    }
}