using impulse_spending_tracker.Data;
using impulse_spending_tracker.Models;
using Microsoft.EntityFrameworkCore;

namespace impulse_spending_tracker.Repositories
{
    public class TagRepository
    {
        private readonly ImpulseSpendingDbContext _dbContext;

        public TagRepository(ImpulseSpendingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IReadOnlyList<Tag> GetAll()
        {
            return _dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Purchases)
                .ToList();
        }

        public Tag? GetById(int id)
        {
            return _dbContext.Tags
                .AsNoTracking()
                .Include(t => t.Purchases)
                .SingleOrDefault(t => t.Id == id);
        }

        public void Create(Tag tag)
        {
            _dbContext.Tags.Add(tag);
            _dbContext.SaveChanges();
        }

        public void Update(Tag tag)
        {
            _dbContext.Tags.Update(tag);
            _dbContext.SaveChanges();
        }

        public void Delete(Tag tag)
        {
            tag.IsDeleted = true;
            _dbContext.Tags.Update(tag);
            _dbContext.SaveChanges();
        }
    }
}
