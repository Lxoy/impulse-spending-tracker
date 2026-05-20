using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class WishlistItemRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public WishlistItemRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<WishlistItem> GetAll()
        {
            return _dbContext.WishlistItems
                .AsNoTracking()
                .Include(w => w.UserProfile)
                .Include(w => w.ConvertedPurchase)
                .ToList();
        }

        public WishlistItem? GetById(int id)
        {
            return _dbContext.WishlistItems
                .AsNoTracking()
                .Include(w => w.UserProfile)
                .Include(w => w.ConvertedPurchase)
                .SingleOrDefault(w => w.Id == id);
        }

        public void Create(WishlistItem item)
        {
            _dbContext.WishlistItems.Add(item);
            _dbContext.SaveChanges();
        }

        public void Update(WishlistItem item)
        {
            _dbContext.WishlistItems.Update(item);
            _dbContext.SaveChanges();
        }

        public void Delete(WishlistItem item)
        {
            item.IsDeleted = true;
            _dbContext.WishlistItems.Update(item);
            _dbContext.SaveChanges();
        }
    }
}
