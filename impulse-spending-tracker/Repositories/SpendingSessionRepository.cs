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
            var sessions = _dbContext.SpendingSessions
                .AsNoTracking()
                .Include(s => s.UserProfile)
                .Include(s => s.Purchases)
                .ToList();

            foreach (var session in sessions)
            {
                session.SpentAmount = session.Purchases.Sum(purchase => purchase.Amount);
            }

            return sessions;
        }

        public SpendingSession? GetById(int id)
        {
            var session = _dbContext.SpendingSessions
                .AsNoTracking()
                .Include(s => s.UserProfile)
                .Include(s => s.Purchases)
                .SingleOrDefault(s => s.Id == id);

            if (session is not null)
            {
                session.SpentAmount = session.Purchases.Sum(purchase => purchase.Amount);
            }

            return session;
        }

        public void Create(SpendingSession session)
        {
            _dbContext.SpendingSessions.Add(session);
            _dbContext.SaveChanges();
        }

        public void Update(SpendingSession session)
        {
            _dbContext.SpendingSessions.Update(session);
            _dbContext.SaveChanges();
        }

        public void Delete(SpendingSession session)
        {
            session.IsDeleted = true;
            _dbContext.SpendingSessions.Update(session);
            _dbContext.SaveChanges();
        }
    }
}
