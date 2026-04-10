using impulse_spending_tracker.Models;

namespace impulse_spending_tracker.Repositories
{
    public class UserProfileMockRepository
    {
        private readonly List<UserProfile> _users;

        public UserProfileMockRepository(List<UserProfile> users)
        {
            _users = users;
        }

        public IReadOnlyList<UserProfile> GetAll()
        {
            return _users;
        }

        public UserProfile? GetById(Guid id)
        {
            return _users.SingleOrDefault(u => u.Id == id);
        }
    }
}