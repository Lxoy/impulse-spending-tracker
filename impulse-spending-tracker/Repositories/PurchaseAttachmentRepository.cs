using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class PurchaseAttachmentRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public PurchaseAttachmentRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<PurchaseAttachment> GetByPurchaseId(int purchaseId)
        {
            return _dbContext.PurchaseAttachments
                .AsNoTracking()
                .Where(a => a.PurchaseId == purchaseId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList();
        }

        public PurchaseAttachment? GetById(int id)
        {
            return _dbContext.PurchaseAttachments
                .AsNoTracking()
                .Include(a => a.Purchase)
                .SingleOrDefault(a => a.Id == id);
        }

        public void Create(PurchaseAttachment attachment)
        {
            _dbContext.PurchaseAttachments.Add(attachment);
            _dbContext.SaveChanges();
        }

        public void Delete(PurchaseAttachment attachment)
        {
            _dbContext.PurchaseAttachments.Remove(attachment);
            _dbContext.SaveChanges();
        }
    }
}
