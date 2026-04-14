using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class WishlistItemMockRepository
    {
        private readonly List<WishlistItem> _wishlistItems;

        public WishlistItemMockRepository(List<UserProfile> users)
        {
            _wishlistItems = users
                .SelectMany(u => u.WishlistItems)
                .ToList();
        }

        public IReadOnlyList<WishlistItem> GetAll()
        {
            return _wishlistItems;
        }

        public WishlistItem? GetById(Guid id)
        {
            return _wishlistItems.SingleOrDefault(w => w.Id == id);
        }
    }
}