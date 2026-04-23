using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class TagMockRepository
    {
        private readonly List<Tag> _tags;

        public TagMockRepository(List<UserProfile> users)
        {
            _tags = users
                .SelectMany(u => u.Purchases)
                .SelectMany(p => p.Tags)
                .DistinctBy(t => t.Id)
                .ToList();
        }

        public IReadOnlyList<Tag> GetAll()
        {
            return _tags;
        }

        public Tag? GetById(int id)
        {
            return _tags.SingleOrDefault(t => t.Id == id);
        }
    }
}