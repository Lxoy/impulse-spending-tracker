using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class MerchantRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public MerchantRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<Merchant> GetAll()
        {
            return _dbContext.Merchants
                .AsNoTracking()
                .Include(m => m.Purchases)
                .ToList();
        }

        public Merchant? GetById(int id)
        {
            return _dbContext.Merchants
                .AsNoTracking()
                .Include(m => m.Purchases)
                .SingleOrDefault(m => m.Id == id);
        }
    }
}
