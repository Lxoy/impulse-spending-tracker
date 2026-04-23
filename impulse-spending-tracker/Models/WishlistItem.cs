using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        public int UserProfileId { get; set; }

        public string Name { get; set; } = string.Empty;
        public decimal DesiredPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public int Priority { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime? TargetPurchaseDate { get; set; }
        public string Reason { get; set; } = string.Empty;
        public bool IsPurchased { get; set; }
        public string LinkUrl { get; set; } = string.Empty;

        public virtual UserProfile? UserProfile { get; set; }
        public virtual Purchase? ConvertedPurchase { get; set; }
    }
}