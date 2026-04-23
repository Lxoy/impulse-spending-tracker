using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace impulse_spending_tracker.Models
{
    public class SpendingSession
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(UserProfile))]
        public int UserProfileId { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string Platform { get; set; } = string.Empty;
        public string Channel { get; set; } = string.Empty;
        public decimal SessionBudget { get; set; }
        public decimal SpentAmount { get; set; }
        public int ItemsViewed { get; set; }
        public int ItemsAddedToCart { get; set; }
        public bool CheckoutCompleted { get; set; }

        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}