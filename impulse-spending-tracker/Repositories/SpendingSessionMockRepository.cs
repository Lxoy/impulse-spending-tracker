using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class SpendingSessionMockRepository
    {
        private readonly List<SpendingSession> _sessions;

        public SpendingSessionMockRepository(List<UserProfile> users)
        {
            _sessions = users
                .SelectMany(u => u.Sessions)
                .ToList();
        }

        public IReadOnlyList<SpendingSession> GetAll()
        {
            return _sessions;
        }

        public SpendingSession? GetById(int id)
        {
            return _sessions.SingleOrDefault(s => s.Id == id);
        }
    }
}