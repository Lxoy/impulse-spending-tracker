using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class TriggerTypeMockRepository
    {
        private readonly List<TriggerType> _tags;

        public TriggerTypeMockRepository(List<UserProfile> users)
        {
            _tags = users
                .SelectMany(u => u.Purchases)
                .SelectMany(p => p.TriggerTypes)
                .DistinctBy(t => t.Id)
                .ToList();
        }

        public IReadOnlyList<TriggerType> GetAll()
        {
            return _tags;
        }

        public TriggerType? GetById(int id)
        {
            return _tags.SingleOrDefault(t => t.Id == id);
        }
    }
}