using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class PurchaseRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public PurchaseRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<Purchase> GetAll()
        {
            return _dbContext.Purchases
                .AsNoTracking()
                .Include(p => p.UserProfile)
                .Include(p => p.Merchant)
                .ToList();
        }

        public Purchase? GetById(int id)
        {
            return _dbContext.Purchases
                .AsNoTracking()
                .Include(p => p.UserProfile)
                .Include(p => p.Merchant)
                .Include(p => p.Tags)
                .SingleOrDefault(p => p.Id == id);
        }

        public void Create(Purchase purchase)
        {
            _dbContext.Purchases.Add(purchase);
            _dbContext.SaveChanges();
        }

        public void Update(Purchase purchase)
        {
            _dbContext.Purchases.Update(purchase);
            _dbContext.SaveChanges();
        }

        public void Delete(Purchase purchase)
        {
            purchase.IsDeleted = true;
            _dbContext.Purchases.Update(purchase);
            _dbContext.SaveChanges();
        }
    }
}
