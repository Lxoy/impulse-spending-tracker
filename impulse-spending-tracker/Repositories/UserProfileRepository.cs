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

        // Calculate a behavioral risk score (0-10 integer) based on recent purchase frequency and amounts
        public int CalculateRiskScore(int userId)
        {
            var user = _dbContext.UserProfiles
                .AsNoTracking()
                .SingleOrDefault(u => u.Id == userId);

            if (user is null)
            {
                return 0;
            }

            var now = DateTime.UtcNow;
            var purchases = _dbContext.Purchases
                .AsNoTracking()
                .Where(p => p.UserProfileId == userId && !p.IsDeleted)
                .ToList();

            if (!purchases.Any()) return 0;

            var last90 = purchases.Where(p => p.PurchasedAt >= now.AddDays(-90)).ToList();
            var last30 = purchases.Where(p => p.PurchasedAt >= now.AddDays(-30)).ToList();

            // Frequency: purchases per month (approx using last 90 days)
            double purchasesPerMonth = last90.Count / 3.0;
            // scale to 0-10 (e.g., 0.5 ppm -> 1, 5 ppm -> 10)
            double frequencyScore = Math.Min(10.0, purchasesPerMonth * 2.0);

            // Amount: average amount relative to monthly income
            decimal avgAmount = last90.Any() ? last90.Average(p => p.Amount) : purchases.Average(p => p.Amount);
            decimal income = user.MonthlyNetIncome > 0 ? user.MonthlyNetIncome : 1m;
            double amountRatio = (double)(avgAmount / income);
            // ratio 1.0 (avg == income) -> 10, ratio 0.5 -> 5
            double amountScore = Math.Min(10.0, amountRatio * 10.0);

            // Recency: purchases in last 30 days
            double recencyScore = Math.Min(10.0, last30.Count * 3.33);

            // Weighted composite (0-10)
            double composite = (frequencyScore * 0.5) + (amountScore * 0.4) + (recencyScore * 0.1);
            int finalScore = (int)Math.Round(Math.Clamp(composite, 0.0, 10.0));
            return finalScore;
        }

        public void Create(UserProfile user)
        {
            _dbContext.UserProfiles.Add(user);
            _dbContext.SaveChanges();
        }

        public void Update(UserProfile user)
        {
            _dbContext.UserProfiles.Update(user);
            _dbContext.SaveChanges();
        }

        public void Delete(UserProfile user)
        {
            user.IsDeleted = true;
            _dbContext.UserProfiles.Update(user);
            _dbContext.SaveChanges();
        }
    }
}
