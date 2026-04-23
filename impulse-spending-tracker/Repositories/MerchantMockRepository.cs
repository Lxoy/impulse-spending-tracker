using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class MerchantMockRepository
    {
        private readonly List<Merchant> _merchants;

        public MerchantMockRepository(List<UserProfile> users)
        {
            _merchants = users
                .SelectMany(u => u.Purchases)
                .Select(p => p.Merchant)
                .OfType<Merchant>()
                .DistinctBy(m => m.Id)
                .ToList();
        }

        public IReadOnlyList<Merchant> GetAll()
        {
            return _merchants;
        }

        public Merchant? GetById(int id)
        {
            return _merchants.SingleOrDefault(m => m.Id == id);
        }
    }
}