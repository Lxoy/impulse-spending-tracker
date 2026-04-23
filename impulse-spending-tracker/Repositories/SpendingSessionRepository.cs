using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class SpendingSessionRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public SpendingSessionRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<SpendingSession> GetAll()
        {
            return _dbContext.SpendingSessions
                .AsNoTracking()
                .Include(s => s.UserProfile)
                .ToList();
        }

        public SpendingSession? GetById(int id)
        {
            return _dbContext.SpendingSessions
                .AsNoTracking()
                .Include(s => s.UserProfile)
                .Include(s => s.Purchases)
                .SingleOrDefault(s => s.Id == id);
        }
    }
}
