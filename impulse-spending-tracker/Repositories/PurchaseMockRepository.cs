using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class PurchaseMockRepository
    {
        private readonly List<Purchase> _purchases;

        public PurchaseMockRepository(List<UserProfile> users)
        {
            _purchases = users
                .SelectMany(u => u.Purchases)
                .ToList();
        }

        public IReadOnlyList<Purchase> GetAll()
        {
            return _purchases;
        }

        public Purchase? GetById(Guid id)
        {
            return _purchases.SingleOrDefault(p => p.Id == id);
        }
    }
}