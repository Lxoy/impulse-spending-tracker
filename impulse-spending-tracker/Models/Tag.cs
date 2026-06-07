using System.ComponentModel.DataAnnotations;

namespace impulse_spending_tracker.Models
{
    public class TriggerType
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Trigger type name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Trigger type name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Color is required.")]
        [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid HEX value, e.g. #1F6FEB.")]
        public string ColorHex { get; set; } = "#1F6FEB";

        [StringLength(250, ErrorMessage = "Description can have up to 250 characters.")]
        public string Description { get; set; } = string.Empty;

        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public bool IsDeleted { get; set; }
    }
}