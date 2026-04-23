using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class UserProfileRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public UserProfileRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<UserProfile> GetAll()
        {
            return _dbContext.UserProfiles
                .AsNoTracking()
                .ToList();
        }

        public UserProfile? GetById(int id)
        {
            return _dbContext.UserProfiles
                .AsNoTracking()
                .SingleOrDefault(u => u.Id == id);
        }
    }
}
