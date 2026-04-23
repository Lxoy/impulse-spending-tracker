using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class BudgetPlanMockRepository
    {
        private readonly List<BudgetPlan> _plans;

        public BudgetPlanMockRepository(List<UserProfile> users)
        {
            _plans = users
                .SelectMany(u => u.BudgetPlans)
                .ToList();
        }

        public IReadOnlyList<BudgetPlan> GetAll()
        {
            return _plans;
        }

        public BudgetPlan? GetById(int id)
        {
            return _plans.SingleOrDefault(p => p.Id == id);
        }
    }
}