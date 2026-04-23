using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class BudgetPlanRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public BudgetPlanRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<BudgetPlan> GetAll()
        {
            return _dbContext.BudgetPlans
                .AsNoTracking()
                .Include(p => p.UserProfile)
                .Include(p => p.CoveredPurchases)
                .ToList();
        }

        public BudgetPlan? GetById(int id)
        {
            return _dbContext.BudgetPlans
                .AsNoTracking()
                .Include(p => p.UserProfile)
                .Include(p => p.CoveredPurchases)
                .SingleOrDefault(p => p.Id == id);
        }
    }
}
