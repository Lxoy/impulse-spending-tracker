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
                .Include(p => p.TriggerTypeTag)
                .ToList();
        }

        public Purchase? GetById(int id)
        {
            return _dbContext.Purchases
                .AsNoTracking()
                .Include(p => p.UserProfile)
                .Include(p => p.Merchant)
                .Include(p => p.TriggerTypeTag)
                .Include(p => p.TriggerTypes)
                .SingleOrDefault(p => p.Id == id);
        }

        public Purchase? GetByIdBasic(int id)
        {
            return _dbContext.Purchases
                .AsNoTracking()
                .SingleOrDefault(p => p.Id == id);
        }

        public void Create(Purchase purchase)
        {
            _dbContext.Purchases.Add(purchase);
            _dbContext.SaveChanges();
        }

        public bool IsWishlistItemLinked(int wishlistItemId, int? excludedPurchaseId = null)
        {
            return _dbContext.Purchases
                .IgnoreQueryFilters()
                .Any(purchase =>
                    purchase.WishlistItemId == wishlistItemId &&
                    (!excludedPurchaseId.HasValue || purchase.Id != excludedPurchaseId.Value));
        }

        public decimal GetBudgetPlanSpentAmount(int budgetPlanId, int? excludedPurchaseId = null)
        {
            return _dbContext.Purchases
                .IgnoreQueryFilters()
                .Where(purchase =>
                    purchase.BudgetPlanId == budgetPlanId &&
                    (!excludedPurchaseId.HasValue || purchase.Id != excludedPurchaseId.Value))
                .Select(purchase => (decimal?)purchase.Amount)
                .Sum() ?? 0m;
        }

        public void Update(Purchase purchase)
        {
            var existing = _dbContext.Purchases.SingleOrDefault(p => p.Id == purchase.Id);
            if (existing is null)
            {
                throw new InvalidOperationException($"Purchase with id {purchase.Id} was not found.");
            }

            existing.UserProfileId = purchase.UserProfileId;
            existing.MerchantId = purchase.MerchantId;
            existing.SpendingSessionId = purchase.SpendingSessionId;
            existing.BudgetPlanId = purchase.BudgetPlanId;
            existing.WishlistItemId = purchase.WishlistItemId;
            existing.Title = purchase.Title;
            existing.Amount = purchase.Amount;
            existing.Currency = purchase.Currency;
            existing.PurchasedAt = purchase.PurchasedAt;
            existing.MoodBeforePurchase = purchase.MoodBeforePurchase;
            existing.NeedLevel = purchase.NeedLevel;
            existing.TriggerType = purchase.TriggerType;
            existing.Installments = purchase.Installments;
            existing.Notes = purchase.Notes;

            _dbContext.SaveChanges();
        }

        public void Delete(Purchase purchase)
        {
            var existing = _dbContext.Purchases.SingleOrDefault(item => item.Id == purchase.Id);
            if (existing is null)
            {
                throw new InvalidOperationException($"Purchase with id {purchase.Id} was not found.");
            }

            existing.IsDeleted = true;
            _dbContext.SaveChanges();
        }
    }
}
